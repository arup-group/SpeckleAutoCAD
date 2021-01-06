using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using Newtonsoft.Json;
using SpeckleAutoCAD;
using SpeckleAutoCAD.DTO;

namespace SpeckleAutoCADApp
{
    public class AutocadDataService
    {
        public static SpeckleObject GetObject(long handle)
        {
            var request = new Request
            {
                Operation = Operation.GetObject,
                Data = handle.ToString()
            };

            var response = DataPipeClient.SendRequest(request);
            if (response.StatusCode != 200)
            {
                throw new Exception(response.Data);
            }

            var dto = JsonConvert.DeserializeObject<DTO>(response.Data);
            switch (dto.ObjectType)
            {
                case Constants.Line:
                    var line = JsonConvert.DeserializeObject<AcadLine>(dto.Data);
                    return new SpeckleLine(line.Coordinates);
                default:
                    return new SpeckleObject();
            }
        }

        public static List<dynamic> GetClients()
        {
            var clients = new List<dynamic>();

            if (DataPipeClient != null)
            {
                var request = new Request
                {
                    Operation = Operation.LoadClientState,
                    Data = string.Empty
                };

                var response = DataPipeClient.SendRequest(request);
                if (!string.IsNullOrEmpty(response.Data))
                {
                    clients = JsonConvert.DeserializeObject<List<dynamic>>(response.Data);
                }
            }

            return clients;
        }

        public static List<SpeckleStream> GetStreams()
        {
            var streams = new List<SpeckleStream>();

            if (DataPipeClient != null)
            {
                var request = new Request
                {
                    Operation = Operation.LoadStreamState,
                    Data = string.Empty
                };

                var response = DataPipeClient.SendRequest(request);
                if (!string.IsNullOrEmpty(response.Data))
                {
                    streams = JsonConvert.DeserializeObject<List<SpeckleStream>>(response.Data);
                }
            }

            return streams;
        }

        public static DataPipeClient DataPipeClient { get; set; }
    }
}
