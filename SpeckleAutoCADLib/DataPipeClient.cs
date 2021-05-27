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
    public class DataPipeClient
    {
        public DataPipeClient(string sInputPipeHandle, string sOutputPipeHandle)
        {
            pipeClientIn = new AnonymousPipeClientStream(PipeDirection.In, sInputPipeHandle);
            streamReader = new StreamReader(pipeClientIn);
            pipeClientOut = new AnonymousPipeClientStream(PipeDirection.Out, sOutputPipeHandle);
            streamWriter = new StreamWriter(pipeClientOut);
            streamWriter.AutoFlush = true;
        }

        public Response SendRequest(Request request)
        {
            var sRequest = JsonConvert.SerializeObject(request);
            string sResponse;

            lock (lockObject)
            {
                streamWriter.WriteLine(sRequest);
                sResponse = streamReader.ReadLine();
            }
            
            var response = JsonConvert.DeserializeObject<Response>(sResponse);
            return response;
        }

        private PipeStream pipeClientIn;
        private PipeStream pipeClientOut;
        private StreamReader streamReader;
        private StreamWriter streamWriter;
        private readonly object lockObject = new Object();
    }
}
