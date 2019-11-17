﻿using System;
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
            //transport.Routing().RouteToEndpoint(typeof(TestCommand), "SimpleRabbitMQ.Endpoint");
            //transport.Routing().RegisterPublisher();
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