using NServiceBus;
using NServiceBus.ConsistencyGuarantees;
using NServiceBus.Settings;

namespace SimpleRabbitMQ.Endpoint1
{
    class ConfigureTransactions : IWantToRunBeforeConfigurationIsFinalized
    {
        public void Run(SettingsHolder settings)
        {
            //neither of these work
            //var foo1 = settings.Get("Transactions.SuppressDistributedTransactions");
            //var foo2 = settings.Get("Transactions.DoNotWrapHandlersExecutionInATransactionScope");
            //System.Collections.Generic.KeyNotFoundException
            //    HResult = 0x80131577
            //Message = The given key(Transactions.DoNotWrapHandlersExecutionInATransactionScope) was not present in the dictionary.
            //    Source = NServiceBus.Core
            //StackTrace:
            //at NServiceBus.Settings.SettingsHolder.Get(String key)
            //at SimpleRabbitMQ.Endpoint1.ConfigureTransactions.Run(SettingsHolder settings) in D:\Projects\SimpleRabbitMQTransportUsage\SimpleRabbitMQ.Endpoint1\ConfigureTransactions.cs:line 16
            //at NServiceBus.InitializableEndpoint.ConfigRunBeforeIsFinalized(IEnumerable`1 concreteTypes)
            //at NServiceBus.InitializableEndpoint.< Initialize > d__1.MoveNext()

            //also not working
            //var hey = settings.GetRequiredTransactionModeForReceives();

            //settings.Set("Transactions.SuppressDistributedTransactions", false);
            //settings.Set("Transactions.DoNotWrapHandlersExecutionInATransactionScope", false);
        }
    }
}
