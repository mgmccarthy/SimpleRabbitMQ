using System.Threading.Tasks;
using NServiceBus;

namespace SimpleRabbitMQ.Messages
{
    // ReSharper disable once InconsistentNaming
    public static class IPipelineContextExtensions
    {
        public static Task SendWithImmediateDispatch(this IPipelineContext context, object message)
        {
            var options = new SendOptions();
            options.RequireImmediateDispatch();
            return context.Send(message, options);
        }

        public static Task PublishWithImmediateDispatch(this IPipelineContext context, object message)
        {
            var options = new PublishOptions();
            options.RequireImmediateDispatch();
            return context.Publish(message, options);
        }
    }
}