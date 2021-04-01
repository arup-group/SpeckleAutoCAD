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
        private static Process speckleAutoCADAppProcess;
        private static Thread pipeServerThread;
        private static EventWaitHandle showSpeckleUISignal;
        private static string showSpeckleUISignalId;
        private EventWaitHandle selectionChangedSignal;
        private string selectionChangedSignalId;

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

                LaunchUI();


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

        private void OnImpliedSelectionChanged(object sender, EventArgs e)
        {
            selectionChangedSignal.Set();
        }

        //For the moment all communications are initiated by the UI so we use these signals to trigger the UI as appropriate 
        private void CreateWaitHandles()
        {
            showSpeckleUISignalId = Guid.NewGuid().ToString("N");
            showSpeckleUISignal = new EventWaitHandle(false, EventResetMode.AutoReset, showSpeckleUISignalId);
            selectionChangedSignalId = Guid.NewGuid().ToString("N");
            selectionChangedSignal = new EventWaitHandle(false, EventResetMode.AutoReset, selectionChangedSignalId);
        }

        private void LaunchUI()
        {
            if (launched == false)
            {
                var requestProcessor = new RequestProcessor();
                var dataPipeServer = new DataPipeServer(requestProcessor.ProcessRequest);
                pipeServerThread = new Thread(dataPipeServer.Run);
                pipeServerThread.Start();

                CreateWaitHandles();

                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = GetAssemblyDirectory() + "\\SpeckleAutoCADApp",
                    Arguments = $"{dataPipeServer.ClientInputHandle} {dataPipeServer.ClientOutputHandle} {showSpeckleUISignalId} {selectionChangedSignalId}",
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
            }
            else
            {
                // Signal Speckle UI application to show
                showSpeckleUISignal.Set();
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
