﻿using NServiceBus;
using NServiceBus.Settings;

namespace SimpleRabbitMQ.Endpoint1
{
    class ConfigureTransactions : IWantToRunBeforeConfigurationIsFinalized
    {
        public void Run(SettingsHolder settings)
        {
            settings.Set("Transactions.SuppressDistributedTransactions", true);
            settings.Set("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
        }
    }
}