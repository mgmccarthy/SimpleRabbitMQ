using System;
using NServiceBus;
using NServiceBus.Persistence;

namespace SimpleRabbitMQ.Endpoint2
{
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(EndpointConfiguration endpointConfiguration)
        {
            const string endpointName = "SimpleRabbitMQ.Endpoint2";
            endpointConfiguration.DefineEndpointName(endpointName);

            //var transport = endpointConfiguration.UseMSMQ();
            var transport = endpointConfiguration.UseRabbitMQ(endpointName);
            transport.Transactions(TransportTransactionMode.ReceiveOnly);

            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");

            //var unitOfWorkSettings = endpointConfiguration.UnitOfWork();
            //unitOfWorkSettings.WrapHandlersInATransactionScope();

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");
        }
    }

    public static class EndpointConfigurationExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static TransportExtensions UseRabbitMQ(this EndpointConfiguration endpointConfiguration, string endpointName)
        {
            //RabbitMQ
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("host=localhost");

            //https://docs.particular.net/monitoring/metrics/
            //https://docs.particular.net/monitoring/metrics/install-plugin
            var metrics = endpointConfiguration.EnableMetrics();
            endpointConfiguration.UniquelyIdentifyRunningInstance().UsingNames(endpointName, Environment.MachineName);
            metrics.SendMetricDataToServiceControl("Particular.Rabbitmq.Monitoring", TimeSpan.FromSeconds(10));

            //this does not appear to be an option with this version of RabbitMQ persistence, but apparently, it's the default
            //transport.UseConventionalRoutingTopology();

            return transport;
        }

        // ReSharper disable once InconsistentNaming
        public static TransportExtensions UseMSMQ(this EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<MsmqTransport>();
            var scanner = endpointConfiguration.AssemblyScanner();
            scanner.ExcludeAssemblies("NServiceBus.Transports.RabbitMQ.dll", "RabbitMQ.dll");

            var settings = transport.Routing();
            settings.RegisterPublisher(typeof(SimpleRabbitMQ.Messages.TestEvent), "SimpleRabbitMQ.Endpoint1");

            return transport;
        }
    }
}