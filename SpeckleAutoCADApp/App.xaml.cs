using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SpeckleUiBase;
using SpeckleAutoCADApp.UI;

namespace SpeckleAutoCADApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            var bindings = new SpeckleUIBindingsAutoCAD();

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

