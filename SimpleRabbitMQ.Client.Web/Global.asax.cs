using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using NServiceBus;
using SimpleRabbitMQ.Common;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Client.Web
{
    public class WebApiApplication : HttpApplication
    {
        public static IEndpointInstance Endpoint;

        protected void Application_Start()
        {
            const string endpointName = "SimpleRabbitMQ.Client.Web";
            var endpointConfiguration = new EndpointConfiguration(endpointName);

            //var transport = endpointConfiguration.UseMSMQ();
            var transport = endpointConfiguration.UseRabbitMQ(endpointName);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            transport.Routing().RouteToEndpoint(typeof(TestCommand), "SimpleRabbitMQ.Endpoint1");

            endpointConfiguration.EnableInstallers();
            //endpointConfiguration.EnableOutbox();

            ////if this is not a SendOnly endpoint, then we need to set the persistence to something that supports outbox
            //var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            //persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");

            //getting rid of SendOnly, aka, putting a queue under Client.Web seems to have a 1:1 dependency on each http request that is processed by the API controller
            //to be subsequently tied to the processing of that message in the endpoint. With SendOnly, the messages throughput was majorly quicker and the endpoint had to catch
            //up to all the messages put onto the endpoint queue
            endpointConfiguration.SendOnly();

            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");

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