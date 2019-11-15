using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NServiceBus;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Client.Web
{
    public class WebApiApplication : HttpApplication
    {
        public static IEndpointInstance Endpoint;

        protected void Application_Start()
        {
            var endpointConfiguration = new EndpointConfiguration("SimpleRabbitMQ.Client.Web");
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.Routing().RouteToEndpoint(typeof(TestCommand), "SimpleRabbitMQ.Endpoint");
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host=localhost");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.EnableOutbox();
            endpointConfiguration.SendOnly();

            Endpoint = NServiceBus.Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_End()
        {
            Endpoint?.Stop().GetAwaiter().GetResult();
        }
    }
}
