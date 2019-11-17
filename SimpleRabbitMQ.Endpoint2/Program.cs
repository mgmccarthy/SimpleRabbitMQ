using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence;

namespace SimpleRabbitMQ.Endpoint2
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "SimpleRabbitMQ.Endpoint2";
            var endpointConfiguration = new EndpointConfiguration("SimpleRabbitMQ.Endpoint2");
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host=localhost");
            endpointConfiguration.EnableInstallers();
            //endpointConfiguration.EnableOutbox();

            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");

            var endpointInstance = await NServiceBus.Endpoint.Start(endpointConfiguration).ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}
