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

namespace SpeckleAutoCADApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args.Length != 2)
            {
                this.Shutdown();
                return;
            }

            var dataPipeClient = new DataPipeClient(e.Args[0], e.Args[1]);
            var bindings = new SpeckleUIBindingsAutoCAD(dataPipeClient);

            // Create main application window, starting minimized if specified
            SpeckleWindow = new SpeckleUiWindow(bindings, @"https://appui.speckle.systems/#/");

            SpeckleWindow.Show();
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

        public SpeckleUIBindingsAutoCAD uiBindings;
        public SpeckleUiWindow SpeckleWindow;
    }
}

