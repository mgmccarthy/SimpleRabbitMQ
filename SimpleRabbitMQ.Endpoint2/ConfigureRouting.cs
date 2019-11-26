using NServiceBus;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint2
{
    //public class ConfigureRouting : INeedInitialization
    //{
    //    public void Customize(EndpointConfiguration configuration)
    //    {
    //        var settings = configuration.UseTransport<RabbitMQTransport>().Routing();
    //        AddMessageRoutesTo(settings);
    //    }

    //    private static void AddMessageRoutesTo(RoutingSettings<RabbitMQTransport> settings)
    //    {
    //        settings.RouteToEndpoint(typeof(TestCommand), "Samples.RabbitMQ.Simple");
    //    }
    //}
}
