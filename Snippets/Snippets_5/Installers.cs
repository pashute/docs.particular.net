﻿using NServiceBus;

public class ForInstallationOnReplacement
{
    public void Simple()
    {
        #region Installers

        BusConfiguration configuration = new BusConfiguration();

        configuration.EnableInstallers();

        Bus.Create(configuration);//this will run the installers

        #endregion
    }

}