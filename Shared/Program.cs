using System;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
//using SpeckleAutoCAD.UI;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Autodesk.AutoCAD.EditorInput;

namespace SpeckleAutoCAD
{
    public class Program : IExtensionApplication
    {
        private static bool launched = false;
        private static int launchingCount = 0;
        //private static SpeckleAutoCADWindow speckleAutoCADWindow;
        private static Process speckleAutoCADAppProcess;
        //private static SpeckleAutoCADAppWindowHost speckleAutoCADAppWindowHost;
        private static Thread pipeServerThread;
        private static EventWaitHandle ewh;
        private static string eventWaitName;

        #region IExtensionApplication Members



        public void Initialize()
        {
 //           throw new System.Exception("The method or operation is not implemented.");
        }

        public void Terminate()
        {
            try
            {
                StopBackgroungProcesses();
            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n" + ex.Message);
            }
        }

        #endregion  

        [CommandMethod("Speckle")]
        public void Speckle()
        {
            try
            {
                launchingCount += 1;
                if (launchingCount > 1)
                {
                    return;
                }

                if (launched == false)
                {
                    //var document = Application.DocumentManager.MdiActiveDocument;
                    //var civilDocument = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
                    var requestProcessor = new RequestProcessor();
                    var dataPipeServer = new DataPipeServer(requestProcessor.ProcessRequest);
                    pipeServerThread = new Thread(dataPipeServer.Run);
                    pipeServerThread.Start();

                    eventWaitName = Guid.NewGuid().ToString("N");
                    ewh = new EventWaitHandle(false, EventResetMode.AutoReset, eventWaitName);
                    //ObjectIdCollection alignments = doc.GetAlignmentIds();
                    //ObjectIdCollection sites = doc.GetSiteIds();
                    //String docInfo = String.Format("\nHello Speckle!");
                    //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(docInfo);
                    //SpeckleWindow = new SpeckleUiWindow(bindings, @"https://appui.speckle.systems/#/");

                    //var helper = new System.Windows.Interop.WindowInteropHelper(SpeckleWindow);
                    //helper.Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

                    //SpeckleWindow.Show();
                    ProcessStartInfo psi = new ProcessStartInfo()
                    {
                        FileName = GetAssemblyDirectory() + "\\SpeckleAutoCADApp",
                        Arguments = $"{dataPipeServer.ClientInputHandle} {dataPipeServer.ClientOutputHandle} {eventWaitName}",
                        UseShellExecute = false
                    };

                    speckleAutoCADAppProcess = Process.Start(psi);
                    WaitForAppWindow();
                    if (speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero)
                    {
                        throw new System.Exception("Unable to initialize Speckle");
                    }

                    SetWindowLongPtr(
                        speckleAutoCADAppProcess.MainWindowHandle,
                        -8,
                        System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
                    );

                    launched = true;
                    //speckleAutoCADAppWindowHost = new SpeckleAutoCADAppWindowHost(speckleAutoCADAppProcess.MainWindowHandle);
                    //speckleAutoCADWindow = new SpeckleAutoCADWindow(speckleAutoCADAppWindowHost);
                    //speckleAutoCADWindow.Show();

                    //string state;
                    //var data = System.IO.File.ReadAllText(@"c:\temp\text1.txt");
                    //Helpers.SpeckleStateManager.WriteState(document, Constants.SpeckleAutoCADStreamsKey, data);
                    //data = Helpers.SpeckleStateManager.ReadState(document, Constants.SpeckleAutoCADStreamsKey);
                    //System.IO.File.WriteAllText(@"c:\temp\text2.txt", data);

                    //var pr = new SpeckleAutoCAD.Helpers.ProgressReporter();
                    //System.Threading.Tasks.Task.Run(
                    //    () =>
                    //    {
                    //        pr.ReportProgress(() =>
                    //        {
                    //            var data = System.IO.File.ReadAllText(@"c:\temp\text1.txt");
                    //            Helpers.SpeckleStateManager.WriteState(document, Constants.SpeckleAutoCADStreamsKey, data);
                    //        });
                    //    });
                }
                else
                {
                    // Signal Speckle UI application to show
                    ewh.Set();
                }


            }
            catch (System.Exception ex)
            {
                Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage( "\n" + ex.Message);
            }
            finally
            {
                launchingCount -= 1;
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
            var startTime = DateTime.Now;

            do
            {
                System.Windows.Forms.Application.DoEvents();
                if (DateTime.Now > startTime.AddSeconds(5))
                {
                    break;
                }
            } while ((speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero) && (!speckleAutoCADAppProcess.HasExited));
        }

        private void StopBackgroungProcesses()
        {
            try
            {
                if (speckleAutoCADAppProcess != null && !speckleAutoCADAppProcess.HasExited)
                {
                    speckleAutoCADAppProcess.Kill();
                }

                if (pipeServerThread != null && pipeServerThread.IsAlive)
                {
                    pipeServerThread.Abort();
                }
            }
            catch
            {

            }
           
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, UInt32 wMsg, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        public const UInt32 WM_SHOWWINDOW = 0x0018;
        private const UInt32 WM_SYSCOMMAND = 0x0112;
        private const UInt32 SC_RESTORE = 0xF120;
        private const UInt32 WM_PAINT = 0xF;

    }
}
