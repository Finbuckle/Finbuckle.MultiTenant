/* 
 * removed because of the following error:
 * System.NotSupportedException: 'Unsupported expression: x => x.UseConsumeFilter(Finbuckle.MultiTenant.MassTransit.MassTransitFilters.TenantConsumeFilter`1[T], registrationContextMock.Object) Extension methods (here: DependencyInjectionFilterExtensions.UseConsumeFilter) may not be used in setup / verification expressions.'
 * This means that Moq does not support extension methods. Thus we just need to remove the test. or look for another way to test it.
 * such as with Microsoft Fakes or alternatives.
 * However we test this extension method in the test ./Finbuckle.MultiTenant.MassTransit.Test/MassTransitFilters/TenantConsumeFilterShould.cs test
 * and others using an in memory service bus, utilising MassTransit's test harness.
 */


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using Finbuckle.MultiTenant.MassTransit.MassTransitFilters;

//using MassTransit;
//using MassTransit.Configuration;

//using Moq;

//namespace Finbuckle.MultiTenant.MassTransit.Test.Extensions
//{
//    public  class BusFactoryConfiguratorShould
//    {
//        [Fact]
//        public void AddTenantFilters_RegistersExpectedFilters()
//        {
//            // Arrange
//            var configuratorMock = new Mock<IBusFactoryConfigurator>();
//            var registrationContextMock = new Mock<IRegistrationContext>();

//            // Act
//            configuratorMock.Object.AddTenantFilters(registrationContextMock.Object);

//            // Assert
//            configuratorMock.Verify(x => x.UseConsumeFilter(typeof(TenantConsumeFilter<>), registrationContextMock.Object), Times.Once);
//            configuratorMock.Verify(x => x.UseSendFilter(typeof(TenantSendFilter<>), registrationContextMock.Object), Times.Once);
//            configuratorMock.Verify(x => x.UsePublishFilter(typeof(TenantPublishFilter<>), registrationContextMock.Object), Times.Once);
//            configuratorMock.Verify(x => x.UseExecuteActivityFilter(typeof(TenantExecuteFilter<>), registrationContextMock.Object), Times.Once);
//            configuratorMock.Verify(x => x.UseCompensateActivityFilter(typeof(TenantCompensateFilter<>), registrationContextMock.Object), Times.Once);
//        }
//    }
//}
