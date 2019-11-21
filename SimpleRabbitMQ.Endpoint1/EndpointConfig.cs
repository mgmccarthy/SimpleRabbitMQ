namespace SimpleRabbitMQ.Endpoint1
{
    using NServiceBus;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint1";

            endpointConfiguration.DefineEndpointName(endpointName);

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            //Rabbit's default transaction level
            //if you try to use this, the endpoint throws and exception on startup
            //transport.Transactions(TransportTransactionMode.TransactionScope);
            //transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
            transport.ConnectionString("host=localhost");
            
            endpointConfiguration.UsePersistence<InMemoryPersistence>();

            var unitOfWorkSettings = endpointConfiguration.UnitOfWork();
            unitOfWorkSettings.WrapHandlersInATransactionScope();

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");
        }
    }
}
