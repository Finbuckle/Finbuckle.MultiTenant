using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Finbuckle.MultiTenant.AzureFunctions.Host.Middleware
{
    public delegate Task HttpMiddlewareDelegate(HttpContext context, RequestDelegate next);
    public static class FunctionsHttpMiddlewareExtensions
    {
        #region JobHostHttpMiddleware
        private const string IJobHostHttpMiddleware = "Microsoft.Azure.WebJobs.Script.Middleware.IJobHostHttpMiddleware";

        private static Type _jobHostHttpMiddlewareType = null;
        private static Type IJobHostHttpMiddlewareType
        {
            get
            {
                if (_jobHostHttpMiddlewareType == null)
                {
                    _jobHostHttpMiddlewareType = AppDomain.CurrentDomain
                        .GetAssemblies()
                        .SelectMany(t => t.GetTypes())
                        .FirstOrDefault(t => t.IsInterface && t.FullName == IJobHostHttpMiddleware);
                }

                return _jobHostHttpMiddlewareType;
            }
        }

        #endregion

        #region ModuleBuilder

        private static ModuleBuilder _moduleBuilder = null;
        private static ModuleBuilder ModuleBuilder
        {
            get
            {
                if (_moduleBuilder == null)
                {
                    var assemblyName = new AssemblyName(RuntimeModulesNamespace);
                    var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                    _moduleBuilder = assemblyBuilder.DefineDynamicModule(RuntimeModulesNamespace);
                }

                return _moduleBuilder;
            }
        }

        #endregion

        #region Private Methods

        private const string RuntimeModulesNamespace = "Finbuckle.MultiTenant.AzureFunctions.Host.Extensions.RuntimeModules";
        private static string GetMiddlewareFullName(string middlewareName) => $"{RuntimeModulesNamespace}.{middlewareName}";

        private static bool TypeExists(string middlewareName)
        {
            var type = ModuleBuilder.GetType(GetMiddlewareFullName(middlewareName), ignoreCase: true);
            return (type != null);
        }

        #endregion

        public static IServiceCollection AddHttpMiddleware(
            this IServiceCollection services,
            string middlewareName,
            HttpMiddlewareDelegate @delegate)
        {
            if (TypeExists(middlewareName))
                throw new ArgumentException($"The middleware name '{middlewareName}' already exists in the dynamic module");

            #region Dynamic Middleware Building

            /************************************************************************
             * Heavily inspired from : https://stackoverflow.com/questions/21500712/how-do-i-convert-an-expression-into-a-methodbuilder-instance-method#answer-21500713
             * The next lines are equivalent to :
             * public class MyMiddleware : IJobHostHttpMiddleware {
             *   private HttpMiddlewareDelegate delegate_field;
             * 
             *   public virtual Task Invoke(HttpContext context, RequestDelegate next) 
             *     => delegate_field(context, next);
             * }
             ************************************************************************/

            var typeBuilder = ModuleBuilder.DefineType(
                GetMiddlewareFullName(middlewareName),
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout,
                null
            );
            typeBuilder.AddInterfaceImplementation(IJobHostHttpMiddlewareType);

            // Create a method builder 
            var @parameters = new Type[2] { typeof(HttpContext), typeof(RequestDelegate) };
            var methodBuilder =
               typeBuilder.DefineMethod(
                   "Invoke",
                   MethodAttributes.Public | MethodAttributes.Virtual,
                   typeof(Task),
                   @parameters
            );

            // Create a field to hold the dynamic delegate
            var fieldBuilder = typeBuilder.DefineField(
                "<>delegate_field",
                typeof(HttpMiddlewareDelegate),
                FieldAttributes.Private
            );

            var il = methodBuilder.GetILGenerator();

            // Push the delegate onto the stack ...
            il.Emit(OpCodes.Ldarg_0);
            // ... by loading the field
            il.Emit(OpCodes.Ldfld, fieldBuilder);

            // Push each argument onto the stack (thus "forwarding" the arguments to the delegate).
            for (int i = 0; i < @parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldarg, i + 1);
            }

            // Call the delegate and return
            il.Emit(OpCodes.Callvirt, typeof(HttpMiddlewareDelegate).GetMethod("Invoke"));
            il.Emit(OpCodes.Ret);

            #endregion

            var middlewareType = typeBuilder.CreateType();
            var instance = Activator.CreateInstance(middlewareType);
            middlewareType
                .GetField("<>delegate_field", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(instance, @delegate);

            // Register middleware type as a singleton
            services.Add(
                ServiceDescriptor.Singleton(IJobHostHttpMiddlewareType, instance)
            );

            return services;
        }
    }
}