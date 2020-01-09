using System;
using NServiceBus;
using NServiceBus.Persistence;

namespace SimpleRabbitMQ.Common
{
    public static class EndpointConfigurationExtensions
    {
        //TODO: maybe this method can return the endpointConfiguration back to the caller so further refinement can be done?
        public static void CommonConfiguration(this EndpointConfiguration endpointConfiguration, string endpointName, string transportSelection = "RabbitMQ")
        {
            var transport = transportSelection == "RabbitMQ" ? 
                endpointConfiguration.UseRabbitMQ(endpointName) : 
                endpointConfiguration.UseMSMQ();
            transport.Transactions(TransportTransactionMode.ReceiveOnly);

            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            //persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");
            //put the NSB persistence in the same datastore as the business data
            persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=SimpleRabbitMQ; Integrated Security=True;");

            //var outboxSettings = endpointConfiguration.EnableOutbox();

            //2019-12-03 15:53:54.984 FATAL NServiceBus.GenericHost Exception when starting endpoint.
            //System.InvalidOperationException: NServiceBus performance counter for 'Critical Time' is not set up correctly.To rectify this problem, consult the NServiceBus performance counters documentation. --->System.InvalidOperationException: The requested Performance Counter is not a custom counter, it has to be initialized as ReadOnly.
            //PerformanceCounters has been moved to an external nuget package: NServiceBus.Metrics.PerformanceCounters
            //Old PerformanceCounter APIs marked obsolete in 6.2 of NServiceBus
            //#pragma warning disable 618
            //endpointConfiguration.EnableCriticalTimePerformanceCounter();
            //endpointConfiguration.EnableSLAPerformanceCounter(TimeSpan.FromSeconds(100));

            var performanceCounters = endpointConfiguration.EnableWindowsPerformanceCounters();
            performanceCounters.EnableSLAPerformanceCounters(TimeSpan.FromSeconds(100));

            endpointConfiguration.EnableInstallers();

            endpointConfiguration.SendFailedMessagesTo("SimpleRabbitMQ.Error");
            endpointConfiguration.AuditProcessedMessagesTo("SimpleRabbitMQ.Audit");
        }

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
            transport.UseConventionalRoutingTopology();

            return transport;
        }

        // ReSharper disable once InconsistentNaming
        public static TransportExtensions UseMSMQ(this EndpointConfiguration endpointConfiguration)
        {
            var transport = endpointConfiguration.UseTransport<MsmqTransport>();
            var scanner = endpointConfiguration.AssemblyScanner();
            scanner.ExcludeAssemblies("NServiceBus.Transports.RabbitMQ.dll", "RabbitMQ.dll");

            return transport;
        }
    }
}