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
                    return JsonConvert.DeserializeObject<SpeckleLine>(dto.Data);
                default:
                    return new SpeckleObject();
            }
        }

        public static DataPipeClient DataPipeClient { get; set; }
    }
}
