using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using SpeckleAutoCAD.UI;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SpeckleAutoCAD
{
    public class Program : IExtensionApplication
    {
        private static bool launched = false;
        private static SpeckleAutoCADWindow speckleAutoCADWindow;
        private static Process speckleAutoCADAppProcess;
        private static SpeckleAutoCADAppWindowHost speckleAutoCADAppWindowHost;

        #region IExtensionApplication Members



        public void Initialize()
        {
 //           throw new System.Exception("The method or operation is not implemented.");
        }

        public void Terminate()
        {
 //           throw new System.Exception("The method or operation is not implemented.");
        }

        #endregion  

        [CommandMethod("Speckle")]
        public void Speckle()
        {
            try
            {
                CivilDocument doc = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;

                var bindings = new SpeckleUIBindingsAutoCAD(doc);

                ObjectIdCollection alignments = doc.GetAlignmentIds();
                ObjectIdCollection sites = doc.GetSiteIds();
                String docInfo = String.Format("\nHello Speckle!");
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(docInfo);
                //SpeckleWindow = new SpeckleUiWindow(bindings, @"https://appui.speckle.systems/#/");

                //var helper = new System.Windows.Interop.WindowInteropHelper(SpeckleWindow);
                //helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                //SpeckleWindow.Show();

                if (launched == false)
                {
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = GetAssemblyDirectory() + "\\SpeckleAutoCADApp",
                        UseShellExecute = false
                    };

                    speckleAutoCADAppProcess = Process.Start(psi);
                    speckleAutoCADAppProcess.WaitForInputIdle();
                    do
                    {
                        if (speckleAutoCADAppProcess.MainWindowHandle != IntPtr.Zero)
                        {
                            break;
                        }

                        System.Threading.Thread.Sleep(100);

                    } while (speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero);

                    if (speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero)
                    {
                        throw new System.Exception("Unable to initialize Speckle");
                    }

                    speckleAutoCADAppWindowHost = new SpeckleAutoCADAppWindowHost(speckleAutoCADAppProcess.MainWindowHandle);
                    speckleAutoCADWindow = new SpeckleAutoCADWindow(speckleAutoCADAppWindowHost);
                    speckleAutoCADWindow.Show();
                    launched = true;
                }
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage( "\n" + ex.Message);
            }
        }

        public string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
        }
    }
}
