﻿using NServiceBus;

public class SqlServerConfigurationSettings
{
    void TransactionScope()
    {
        #region sqlserver-config-transactionscope 2

        BusConfiguration configuration = new BusConfiguration();
        configuration.UseTransport<SqlServerTransport>();

        #endregion
    }

    void NativeTransactions()
    {
        #region sqlserver-config-native-transactions 2

        BusConfiguration configuration = new BusConfiguration();
        configuration.UseTransport<SqlServerTransport>();
        configuration.Transactions()
            .DisableDistributedTransactions();

        #endregion
    }

    void NoTransactions()
    {
        #region sqlserver-config-no-transactions 2

        BusConfiguration configuration = new BusConfiguration();
        configuration.UseTransport<SqlServerTransport>();
        configuration.Transactions().Disable();

        #endregion
    }

    void DisableSecondaries()
    {
        #region sqlserver-config-disable-secondaries 2

        BusConfiguration configuration = new BusConfiguration();
        configuration.UseTransport<SqlServerTransport>()
            .DisableCallbackReceiver();

        #endregion
    }

    void Callbacks()
    {
        #region sqlserver-config-callbacks 2

        BusConfiguration configuration = new BusConfiguration();
        IStartableBus bus = Bus.Create(configuration);
        ICallback callback = bus.Send(new Request());
        callback.Register(ProcessResponse);
        #endregion

        #region sqlserver-config-callbacks-reply 2

        bus.Return(42);

        #endregion
    }


    void ProcessResponse(CompletionResult returnCode)
    {
    }

    private class Request : IMessage
    {
    }
}