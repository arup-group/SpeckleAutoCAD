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
        private static EventWaitHandle selectionChangedSignal;
        private static string selectionChangedSignalId;
        private static bool quitting;

        #region IExtensionApplication Members



        public void Initialize()
        {
            Application.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged += OnImpliedSelectionChanged;
            Application.DocumentManager.MdiActiveDocument.BeginDocumentClose += OnBeginDocumentClose;
            Application.BeginQuit += OnBeginQuit;
            Application.QuitAborted += OnQuitAborted;
            Application.BeginCloseAll += OnBeginCloseAll;
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
                //launchingCount += 1;
                //if (launchingCount > 1)
                //{
                //    return;
                //}

                //if (launched == false)
                //{
                //    var requestProcessor = new RequestProcessor();
                //    var dataPipeServer = new DataPipeServer(requestProcessor.ProcessRequest);
                //    pipeServerThread = new Thread(dataPipeServer.Run);
                //    pipeServerThread.IsBackground = true;
                //    pipeServerThread.Start();

                //    showSpeckleUISignalId = Guid.NewGuid().ToString("N");
                //    showSpeckleUISignal = new EventWaitHandle(false, EventResetMode.AutoReset, showSpeckleUISignalId);

                //    selectionChangedSignalId = Guid.NewGuid().ToString("N");
                //    selectionChangedSignal = new EventWaitHandle(false, EventResetMode.AutoReset, selectionChangedSignalId);

                //    ProcessStartInfo psi = new ProcessStartInfo()
                //    {
                //        FileName = GetAssemblyDirectory() + "\\SpeckleAutoCADApp",
                //        Arguments = $"{dataPipeServer.ClientInputHandle} {dataPipeServer.ClientOutputHandle} {showSpeckleUISignalId} {selectionChangedSignalId}",
                //        UseShellExecute = false
                //    };

                //    speckleAutoCADAppProcess = Process.Start(psi);
                //    WaitForAppWindow();
                //    if (speckleAutoCADAppProcess.MainWindowHandle == IntPtr.Zero)
                //    {
                //        throw new System.Exception("Unable to initialize Speckle");
                //    }

                //    SetWindowLongPtr(
                //        speckleAutoCADAppProcess.MainWindowHandle,
                //        -8,
                //        System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
                //    );

                //    Application.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged += OnImpliedSelectionChanged;
                //    Application.DocumentManager.MdiActiveDocument.BeginDocumentClose += OnBeginDocumentClose;
                //    Application.BeginQuit += OnBeginQuit;
                //    Application.QuitAborted += OnQuitAborted;

                //    launched = true;
                //}
                //else
                //{
                //    // Signal Speckle UI application to show
                //    showSpeckleUISignal.Set();
                //}


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

        private void OnImpliedSelectionChanged(object sender, EventArgs e)
        {
            if (quitting == false)
            {
                //selectionChangedSignal.Set();
            }
            
        }

        private void OnBeginDocumentClose(object sender, DocumentBeginCloseEventArgs e)
        {
            Application.DocumentManager.MdiActiveDocument.ImpliedSelectionChanged -= OnImpliedSelectionChanged;
        }

        private void OnBeginQuit(object sender, BeginQuitEventArgs e)
        {
            quitting = true;
        }

        private void OnQuitAborted(object sender, EventArgs e)
        {
            quitting = false;
        }

        private void OnBeginCloseAll(object sender, BeginCloseAllEventArgs e)
        {
            quitting = true;
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
