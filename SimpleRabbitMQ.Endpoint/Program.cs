using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence;

namespace SimpleRabbitMQ.Endpoint
{
    class Program
    {
        static async Task Main()
        {
            Console.Title = "SimpleRabbitMQ.Endpoint";
            var endpointConfiguration = new EndpointConfiguration("SimpleRabbitMQ.Endpoint");
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host=localhost");
            endpointConfiguration.EnableInstallers();
            endpointConfiguration.EnableOutbox();

            var persistence = endpointConfiguration.UsePersistence<NHibernatePersistence>();
            persistence.ConnectionString(@"Data Source=(LocalDB)\MSSQLLocalDB; Initial Catalog=NServiceBusNHibernatePersistence; Integrated Security=True;");

            var endpointInstance = await NServiceBus.Endpoint.Start(endpointConfiguration).ConfigureAwait(false);
        
            ////this didn't work. Not too sure if there is a difference in Delayed Delivery with Rabbit?
            //var options = new SendOptions();
            //options.DelayDeliveryWith(TimeSpan.FromSeconds(5000));

            //var testCommand = new TestCommand
            //{
            //    OrderId = Guid.NewGuid(),
            //    ProductName = "ProductName",
            //    Descriptions = new List<string> { "1", "2", "3" }
            //};
            //await endpointInstance.Send(testCommand).ConfigureAwait(false);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            await endpointInstance.Stop().ConfigureAwait(false);
        }
    }
}