using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using SpeckleAutoCAD.UI;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SpeckleAutoCAD
{
    public class Program : IExtensionApplication
    {
        private static bool launched = false;
        private static SpeckleAutoCADWindow speckleAutoCADWindow;
        private static Process speckleAutoCADAppProcess;
        private static SpeckleAutoCADAppWindowHost speckleAutoCADAppWindowHost;
        Thread pipeServerThread;

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
                var document = Application.DocumentManager.MdiActiveDocument;
                var civilDocument = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
                //var requestProcessor = new RequestProcessor(document, civilDocument);
                //var dataPipeServer = new DataPipeServer(requestProcessor.ProcessRequest);
                //pipeServerThread = new Thread(dataPipeServer.Run);
                //pipeServerThread.Start();

                ////ObjectIdCollection alignments = doc.GetAlignmentIds();
                ////ObjectIdCollection sites = doc.GetSiteIds();
                ////String docInfo = String.Format("\nHello Speckle!");
                ////Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(docInfo);
                ////SpeckleWindow = new SpeckleUiWindow(bindings, @"https://appui.speckle.systems/#/");

                ////var helper = new System.Windows.Interop.WindowInteropHelper(SpeckleWindow);
                ////helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                ////SpeckleWindow.Show();
                //ProcessStartInfo psi = new ProcessStartInfo()
                //{
                //    FileName = GetAssemblyDirectory() + "\\SpeckleAutoCADApp",
                //    Arguments = $"{dataPipeServer.ClientInputHandle} {dataPipeServer.ClientOutputHandle}",
                //    UseShellExecute = false
                //};

                //speckleAutoCADAppProcess = Process.Start(psi);
                //speckleAutoCADAppProcess.WaitForInputIdle();
                //WaitForAppWindow();
                //if (speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero)
                //{
                //    throw new System.Exception("Unable to initialize Speckle");
                //}

                //speckleAutoCADAppWindowHost = new SpeckleAutoCADAppWindowHost(speckleAutoCADAppProcess.MainWindowHandle);
                //speckleAutoCADWindow = new SpeckleAutoCADWindow(speckleAutoCADAppWindowHost);
                //speckleAutoCADWindow.ShowDialog();

                string state;
                var data = System.IO.File.ReadAllText(@"c:\temp\text1.txt");
                //Helpers.SpeckleStateManager.WriteState(document, Constants.SpeckleAutoCADStreamsKey, data);
                data = Helpers.SpeckleStateManager.ReadState(document, Constants.SpeckleAutoCADStreamsKey);
                System.IO.File.WriteAllText(@"c:\temp\text2.txt", data);



            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage( "\n" + ex.Message);
            }
            finally
            {
                StopBackgroungProcesses();
            }
        }

        public string GetAssemblyDirectory()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
        }

        private void WaitForAppWindow()
        {
            int timeLeft = 5000;

            do
            {
                if (speckleAutoCADAppProcess.MainWindowHandle != IntPtr.Zero)
                {
                    break;
                }

                System.Threading.Thread.Sleep(100);
                timeLeft -= 100;

            } while ((timeLeft > 0) && (speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero) && (!speckleAutoCADAppProcess.HasExited));

        }

        private void StopBackgroungProcesses()
        {
            try
            {
                if (speckleAutoCADAppProcess != null && !speckleAutoCADAppProcess.HasExited)
                {
                    speckleAutoCADAppProcess.Kill();
                }

                if (pipeServerThread.IsAlive)
                {
                    pipeServerThread.Abort();
                }
            }
            catch
            {

            }
           
        }
    }
}
