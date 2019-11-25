using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.ConsistencyGuarantees;
using NServiceBus.Persistence;

namespace SimpleRabbitMQ.Endpoint1
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint1";

            var settings = endpointConfiguration.GetSettings();
            //this doesn't work
            //var foo = settings.TryGet("GetRequiredTransactionModeForReceives", out TransportTransactionMode ttm);

            endpointConfiguration.DefineEndpointName(endpointName);

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);
            //Rabbit's default transaction level
            //if you try to use this, the endpoint throws and exception on startup
            //transport.Transactions(TransportTransactionMode.TransactionScope);
            //transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);
            transport.ConnectionString("host=localhost");
            //this does not appear to be an option with this version of RabbitMQ persistence, but apparently, it's the default
            //transport.UseConventionalRoutingTopology();

            //var settings = transport.GetSettings();

            //endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");

            var unitOfWorkSettings = endpointConfiguration.UnitOfWork();
            unitOfWorkSettings.WrapHandlersInATransactionScope();

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");

            //var foo = TransactionModeSettingsExtensions.GetRequiredTransactionModeForReceives()
        }
    }
}