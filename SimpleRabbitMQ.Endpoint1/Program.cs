using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence;

namespace SimpleRabbitMQ.Endpoint1
{
    class Program
    {
        static async Task Main()
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint1";

            Console.Title = endpointName;
            var endpointConfiguration = new EndpointConfiguration(endpointName);
            endpointConfiguration.UseSerialization<NewtonsoftSerializer>();
            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");

            //both EP's start and process messages normally. For now, going to keep this here
            var unitOfWorkSettings = endpointConfiguration.UnitOfWork();
            unitOfWorkSettings.WrapHandlersInATransactionScope();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.Transactions(TransportTransactionMode.ReceiveOnly); //Rabbit's default transaction level
            //if you try to use this, the endpoint throws and exception on startup
            //transport.Transactions(TransportTransactionMode.TransactionScope);
            //transport.Transactions(TransportTransactionMode.SendsAtomicWithReceive);

            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host=localhost");
            endpointConfiguration.EnableInstallers();
            //endpointConfiguration.EnableOutbox();

            //var persistence = endpointConfiguration.UsePersistence<InMemoryPersistence>();
            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");

            ////https://docs.particular.net/monitoring/metrics/
            ////https://docs.particular.net/monitoring/metrics/install-plugin
            //var metrics = endpointConfiguration.EnableMetrics();
            //endpointConfiguration.UniquelyIdentifyRunningInstance()
            //    .UsingNames(instanceName: endpointName, hostName: Environment.MachineName);
            //metrics.SendMetricDataToServiceControl(serviceControlMetricsAddress: "Particular.Rabbitmq.Monitoring", interval: TimeSpan.FromSeconds(10));

            //this did nothing to help throughput
            //var timeoutManager = endpointConfiguration.TimeoutManager();
            //timeoutManager.LimitMessageProcessingConcurrencyTo(5);

            var endpointInstance = await NServiceBus.Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}