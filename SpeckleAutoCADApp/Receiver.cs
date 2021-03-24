using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;
using Newtonsoft.Json;
using SpeckleCore;
using System.Reflection;
using System;
using System.Linq;
using SpeckleCore.Data;

namespace SpeckleAutoCADApp.UI
{
    public partial class SpeckleUIBindingsAutoCAD
    {
        public override void AddReceiver(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);
            clients.Add(client);

            var request = new Request
            {
                Operation = Operation.SaveClientState,
                Data = JsonConvert.SerializeObject(clients)
            };

            var response = dataPipeClient.SendRequest(request);
            if (response.StatusCode != 200)
            {
                return;
            }
        }

        public override void BakeReceiver(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);
            var apiClient = new SpeckleApiClient((string)client.account.RestApi) { AuthToken = (string)client.account.Token };

            NotifyUi("update-client", JsonConvert.SerializeObject(new
            {
                _id = (string)client._id,
                loading = true,
                loadingBlurb = "Getting stream from server..."
            }));

            var previousStream = speckleStreams.FirstOrDefault(s => s.StreamId == (string)client.streamId);
            var stream = apiClient.StreamGetAsync((string)client.streamId, "").Result.Resource;

            // If it's the first time we bake this stream, create a local shadow copy
            if (previousStream == null)
            {
                previousStream = new SpeckleStream() { StreamId = stream.StreamId, Objects = new List<SpeckleObject>() };
                speckleStreams.Add(previousStream);
            }

            LocalContext.GetCachedObjects(stream.Objects, (string)client.account.RestApi);
            var payload = stream.Objects.Where(o => o.Type == "Placeholder").Select(obj => obj._id).ToArray();

            // TODO: Orchestrate & save in cache afterwards!
            var objects = apiClient.ObjectGetBulkAsync(payload, "").Result.Resources;

            foreach (var obj in objects)
            {
                stream.Objects[stream.Objects.FindIndex(o => o._id == obj._id)] = obj;
            }
        }
    }
}
