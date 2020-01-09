using NServiceBus;
using SimpleRabbitMQ.Common;

namespace SimpleRabbitMQ.Endpoint2
{
    public class Endpoint2Config : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint2";
            endpointConfiguration.DefineEndpointName(endpointName);
            endpointConfiguration.CommonConfiguration(endpointName, "RabbitMQ");
        }
    }
}