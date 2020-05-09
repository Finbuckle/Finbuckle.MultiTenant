using DataIsolationBlazorSample.Models;
using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using DataIsolationBlazorSample.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace DataIsolationBlazorSample.Classes
{
    public class ContextHelper
    {
        private readonly IHttpContextAccessor _accessor;
        private readonly ToDoDbContext _dbContext;
        public ContextHelper(IHttpContextAccessor accessor, ToDoDbContext dbContext)
        {
            _accessor = accessor;
            _dbContext = dbContext;
        }
        public TenantInfo GetCurrentTenant()
        {
            var context = _accessor.HttpContext;
            var ti = context.GetMultiTenantContext<TenantInfo>().TenantInfo;
            return ti;
        }
        public List<ToDoItem> GetToDoItems()
        {
            var context = _accessor.HttpContext;
            List<ToDoItem> toDoItems = new List<ToDoItem>();
            var v1 = context.GetMultiTenantContext<TenantInfo>();
            if (v1.TenantInfo != null)
            {
                toDoItems = _dbContext.ToDoItems.ToList();
            }
            return toDoItems;
        }
        public string GetTenantName()
        {
            try
            {
                var context = _accessor.HttpContext;
                var v1 = context.GetMultiTenantContext<TenantInfo>();
                if (v1.TenantInfo != null)
                {
                    return v1.TenantInfo.Name;
                }
            }
            catch (Exception x)
            { return @"Exception:Unknown"; }
            return @"Unknown";
        }
    }
    public class ConfigHelper: IConfigHelper
    {
        public ITenantStrategy _tenantStrategy { get; set; }
        public ConfigHelper(ITenantStrategy tenantStrategy)
        {
            _tenantStrategy = new TenantStrategy();
            if (tenantStrategy!=null)
                _tenantStrategy = tenantStrategy;
        }
        public bool IsRouteStrategyUsed()
        {
            if (_tenantStrategy == null) return true;
            return _tenantStrategy.RouteStrategy;
        }
    }

    public interface IConfigHelper
    {
        ITenantStrategy _tenantStrategy{ get; set; }
        bool IsRouteStrategyUsed();
    }
     public class TenantStrategy: ITenantStrategy
    {
            public bool RouteStrategy { get; set; } = true;
            public string DefaultRoute { get; set; } = @"{__tenant__=}/{controller=Home}/{action=Index}";
            public string HostTemplate { get; set; } = @"__tenant__.*";
    }
    public interface ITenantStrategy
    {
        bool RouteStrategy { get; set; }
        string DefaultRoute { get; set; }
        string HostTemplate { get; set; }
    }
}
