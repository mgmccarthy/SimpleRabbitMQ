using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using SimpleRabbitMQ.Messages;

namespace SimpleRabbitMQ.Endpoint2
{
    public class TestEventHandler :IHandleMessages<TestEvent>
    {
        private static readonly ILog Log = LogManager.GetLogger<TestEventHandler>();

        public Task Handle(TestEvent message, IMessageHandlerContext context)
        {
            Log.Info($"TestEventHandler. OrderId: {message.OrderId}");
            return Task.CompletedTask;
        }
    }
}
