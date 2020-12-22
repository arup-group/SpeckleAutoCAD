using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;
using Newtonsoft.Json;
using SpeckleCore;
using System.Reflection;
using System;
using System.Linq;

namespace SpeckleAutoCADApp.UI
{
    public partial class SpeckleUIBindingsAutoCAD
    {
        public override void AddSender(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);
            clients.Add(client);
            var speckleStream = new SpeckleStream() { StreamId = (string)client.streamId, Objects = new List<SpeckleObject>() };
            speckleStreams.Add(speckleStream);

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

            request.Operation = Operation.SaveStreamState;
            request.Data = JsonConvert.SerializeObject(speckleStreams);
            response = dataPipeClient.SendRequest(request);

            ISelectionFilter filter = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(client.filter), GetFilterType(client.filter.Type.ToString()));
            //GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());
            GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());
        }

        private Type GetFilterType(string typeString)
        {
            Assembly ass = typeof(ISelectionFilter).Assembly;
            return ass.GetType(typeString);
        }

        private IEnumerable<SpeckleObject> GetSelectionFilterObjects(ISelectionFilter filter, string clientId, string streamId)
        {
            Request request;
            Response response;
            IEnumerable<SpeckleObject> objects = new List<SpeckleObject>();
            var selectionIds = new List<long>();

            try
            {
                if (dataPipeClient != null) return objects;

                if (filter.Name == "Selection")
                {

                }
                else if (filter.Name == "Category")
                {
                    var catFilter = filter as ListSelectionFilter;
                    foreach (var cat in catFilter.Selection)
                    {
                        switch (cat)
                        {
                            case Constants.Line:
                                request = new Request
                                {
                                    Operation = Operation.GetAllLineIds,
                                    Data = string.Empty
                                };

                                response = dataPipeClient.SendRequest(request);
                                if (!string.IsNullOrEmpty(response.Data))
                                {
                                    selectionIds.AddRange(JsonConvert.DeserializeObject<List<long>>(response.Data));
                                }
                                break;
                        }
                    }
                }

                objects = selectionIds.Select(id =>
                {
                    var temp = new SpeckleObject();
                    temp.Properties["autocadhandle"] = id;
                    temp.Properties["__type"] = "Sent Object";
                    return temp;
                });

                var myStream = speckleStreams.FirstOrDefault(st => st.StreamId == streamId);
                myStream.Objects.Clear();
                myStream.Objects.AddRange(objects);

                var myClient = clients.FirstOrDefault(cl => (string)cl._id == (string)clientId);
                myClient.objects = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(myStream.Objects));

                // Persist state and clients to revit file
                request = new Request
                {
                    Operation = Operation.SaveClientState,
                    Data = JsonConvert.SerializeObject(clients)
                };

                response = dataPipeClient.SendRequest(request);

                request = new Request
                {
                    Operation = Operation.SaveStreamState,
                    Data = JsonConvert.SerializeObject(speckleStreams)
                };

                response = dataPipeClient.SendRequest(request);

                var plural = objects.Count() == 1 ? "" : "s";
                if (objects.Count() != 0)
                    NotifyUi("update-client", JsonConvert.SerializeObject(new
                    {
                        _id = clientId,
                        expired = true,
                        objects = myClient.objects,
                        //message = $"You have added {objects.Count()} object{plural} to this sender."
                    }));
            }
            catch
            {

            }

            return objects;
        }
    }
}
