using NServiceBus.ConsistencyGuarantees;
using NServiceBus.Features;

namespace SimpleRabbitMQ.Endpoint1
{
    public class TransactionFeature : Feature
    {
        public TransactionFeature()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            var consistency = context.Settings.GetRequiredTransactionModeForReceives();
            context.Settings.TryGet("Transactions.SuppressDistributedTransactions", out bool suppressDistributedTransactions);
            context.Settings.TryGet("Transactions.DoNotWrapHandlersExecutionInATransactionScope", out bool doNotWrapHandlersExecutionInATransactionScope);
        }
    }
}
