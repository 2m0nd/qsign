using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace UIQuickSignum
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
	    private NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            Log.Info("OnStartup application version 1.2.0.0");
            this.DispatcherUnhandledException += OnDispatcherUnhandledException;
            base.OnStartup(e);
            Log.Info("Finish OnStartup");
        }

	    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs arg)
	    {
	        Log.Error(arg.Exception);
	    }
	}
}
