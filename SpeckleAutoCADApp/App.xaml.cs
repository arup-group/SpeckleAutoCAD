using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SpeckleUiBase;
using SpeckleAutoCADApp.UI;
using SpeckleAutoCAD;
using System.Threading;

namespace SpeckleAutoCADApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            DataPipeClient dataPipeClient;
            SpeckleUIBindingsAutoCAD bindings;

            if (e.Args.Length == 3)
            {
                dataPipeClient = new DataPipeClient(e.Args[0], e.Args[1]);
                AutocadDataService.DataPipeClient = dataPipeClient;
                bindings = new SpeckleUIBindingsAutoCAD(dataPipeClient);

                ewh = EventWaitHandle.OpenExisting(e.Args[2]);
                eventProcessorThread = new Thread(ProcessEvent);
                eventProcessorThread.Start();
            }
            else
            {
                bindings = new SpeckleUIBindingsAutoCAD(null);
            }


            // Create main application window, starting minimized if specified
            SpeckleWindow = new SpeckleUiWindow(bindings, @"https://appui.speckle.systems/#/");
            SpeckleWindow.Show();

            // ewh = new EventWaitHandle(false, EventResetMode.AutoReset, Constants.SpeckleNotificationEventName);
            
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            try
            {

            }
            catch
            {

            }

        }

        private void ProcessEvent()
        {
            while (true)
            {
                ewh.WaitOne();
                SpeckleWindow.Dispatcher.BeginInvoke(
                    new Action(() =>
                    {
                        SpeckleWindow.Show();
                    })
                );
            }
        }

        public SpeckleUIBindingsAutoCAD uiBindings;
        public SpeckleUiWindow SpeckleWindow;
        private Thread eventProcessorThread;
        private EventWaitHandle ewh;
    }
}

