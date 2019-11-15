using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint
{
    public class TestEventHandler :IHandleMessages<TestEvent>
    {
        static ILog log = LogManager.GetLogger<TestEventHandler>();

        public Task Handle(TestEvent message, IMessageHandlerContext context)
        {
            log.Info("Hello from TestEventHandler");
            return Task.CompletedTask;
        }
    }
}
