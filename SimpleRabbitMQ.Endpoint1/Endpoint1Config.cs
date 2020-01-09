using NServiceBus;
using SimpleRabbitMQ.Common;

namespace SimpleRabbitMQ.Endpoint1
{
    public class Endpoint1Config : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint1";
            endpointConfiguration.DefineEndpointName(endpointName);
            endpointConfiguration.CommonConfiguration(endpointName, "RabbitMQ");
        }
    }
}