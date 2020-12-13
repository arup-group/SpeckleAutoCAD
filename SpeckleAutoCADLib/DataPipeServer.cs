using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Pipes;
using Newtonsoft.Json;


namespace SpeckleAutoCAD
{
    public class DataPipeServer
    {
        public DataPipeServer(Func<string, string> callback)
        {
            inputPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
            streamReader = new StreamReader(inputPipe);
            outputPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            streamWriter = new StreamWriter(outputPipe);
            streamWriter.AutoFlush = true;
            this.callback = callback;
        }

        public void Run()
        {
            do
            {
                var request = streamReader.ReadLine();
                var response = callback(request);
                streamWriter.WriteLine(response);
            }
            while (true);
        }

        public string ClientInputHandle
        {
            get
            {
                return outputPipe.GetClientHandleAsString();
            }
        }

        public string ClientOutputHandle
        {
            get
            {
                return inputPipe.GetClientHandleAsString();
            }
        }

        public void ReleaseClientHandles()
        {
            inputPipe.DisposeLocalCopyOfClientHandle();
            outputPipe.DisposeLocalCopyOfClientHandle();
        }

        private AnonymousPipeServerStream inputPipe;
        private AnonymousPipeServerStream outputPipe;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private Func<string, string> callback;
    }
}
