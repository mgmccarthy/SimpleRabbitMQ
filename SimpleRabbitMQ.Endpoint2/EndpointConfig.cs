
namespace SimpleRabbitMQ.Endpoint2
{
    using NServiceBus;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint2";

            endpointConfiguration.DefineEndpointName(endpointName);

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            //Rabbit's default transaction level
            //if you try to use this, the endpoint throws and exception on startup
            //transport.Transactions(TransportTransactionMode.TransactionScope);
            //transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

            endpointConfiguration.UsePersistence<PLEASE_SELECT_ONE>();

            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");
        }
    }
}
