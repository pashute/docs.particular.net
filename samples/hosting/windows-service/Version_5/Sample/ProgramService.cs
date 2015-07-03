using System;
using System.Diagnostics;
using System.ServiceProcess;
using NServiceBus;

class ProgramService : ServiceBase
{
    IBus bus;

    #region windowsservice-hosting-main

    static void Main()
    {
        using (ProgramService service = new ProgramService())
        {
            if (Environment.UserInteractive)
            {
                service.OnStart(null);

                Console.WriteLine("Bus created and configured");
                Console.WriteLine("Press any key to exit");
                Console.ReadKey();

                service.OnStop();

                return;
            }
            Run(service);
        }
    }

    #endregion

    #region windowsservice-hosting-onstart

    protected override void OnStart(string[] args)
    {
        BusConfiguration busConfiguration = new BusConfiguration();

        busConfiguration.EndpointName("Sample.WindowsServiceAndConsole");
        busConfiguration.UseSerialization<JsonSerializer>();

        if (Environment.UserInteractive && Debugger.IsAttached)
        {
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.EnableInstallers();
        }

        bus = Bus.Create(busConfiguration).Start();
    }

    #endregion

    #region windowsservice-hosting-onstop

    protected override void OnStop()
    {
        if (bus != null)
        {
            bus.Dispose();
        }
    }

    #endregion

}