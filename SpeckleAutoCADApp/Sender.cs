﻿using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;
using Newtonsoft.Json;
using SpeckleCore;
using System.Reflection;
using System;
using System.Linq;
using SpeckleCore.Data;
using System.Threading.Tasks;

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
            GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());
        }

        // Send objects to Speckle server. Triggered on "Push!".
        // Create buckets, send sequentially, notify ui re upload progress
        public override void PushSender(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);

            //if it's a category or property filter we need to refresh the list of objects
            //if it's a selection filter just use the objects that were stored previously
            ISelectionFilter filter = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(client.filter), GetFilterType(client.filter.Type.ToString()));
            IEnumerable<SpeckleObject> objects = new List<SpeckleObject>();

            objects = GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());

            var apiClient = new SpeckleApiClient((string)client.account.RestApi) { AuthToken = (string)client.account.Token };
            var task = Task.Run(async () => { await apiClient.IntializeUser(); });
            task.Wait();
            apiClient.ClientType = "AutoCAD";

            var convertedObjects = new List<SpeckleObject>();
            var placeholders = new List<SpeckleObject>();

            var units = AutocadDataService.GetLengthUnit();
            //InjectScaleInKits(GetScale(units)); // this is used for feet to sane units conversion.

            int i = 0;
            long currentBucketSize = 0;
            var errorMsg = "";
            var failedToConvert = 0;
            var errors = new List<SpeckleError>();
            foreach (var obj in objects)
            {
                NotifyUi("update-client", JsonConvert.SerializeObject(new
                {
                    _id = (string)client._id,
                    loading = true,
                    isLoadingIndeterminate = false,
                    loadingProgress = 1f * i++ / objects.Count() * 100,
                    loadingBlurb = string.Format("Converting and uploading objects: {0} / {1}", i, objects.Count())
                }));

                long handle = 0;
                SpeckleObject speckleObject = null;

                try
                {
                    handle = (long)obj.Properties["autocadhandle"];
                    speckleObject = AutocadDataService.GetObject(handle);
                    if (speckleObject == null)
                    {
                        errors.Add(new SpeckleError { Message = "Could not retrieve element", Details = string.Empty });
                        continue;
                    }
                }
                catch (Exception e)
                {
                    errors.Add(new SpeckleError { Message = "Could not retrieve element", Details = e.Message });
                    continue;
                }

                try
                {
                    var conversionResult = new List<SpeckleObject> { speckleObject };
                    var byteCount = Converter.getBytes(conversionResult).Length;
                    currentBucketSize += byteCount;

                    if (byteCount > 2e6)
                    {
                        errors.Add(new SpeckleError { Message = "Element is too big to be sent", Details = $"Element {handle} is bigger than 2MB, it will be skipped" });
                        continue;
                    }

                    convertedObjects.AddRange(conversionResult);

                    if (currentBucketSize > 5e5 || i >= objects.Count()) // aim for roughly 500kb uncompressed
                    {
                        LocalContext.PruneExistingObjects(convertedObjects, apiClient.BaseUrl);

                        try
                        {
                            var chunkResponse = apiClient.ObjectCreateAsync(convertedObjects).Result.Resources;
                            int m = 0;
                            foreach (var objConverted in convertedObjects)
                            {
                                objConverted._id = chunkResponse[m++]._id;
                                placeholders.Add(new SpecklePlaceholder() { _id = objConverted._id });
                                if (objConverted.Type != "Placeholder") LocalContext.AddSentObject(objConverted, apiClient.BaseUrl);
                            }
                        }
                        catch (Exception e)
                        {
                            errors.Add(new SpeckleError { Message = $"Failed to send {convertedObjects.Count} objects", Details = e.Message });
                        }
                        currentBucketSize = 0;
                        convertedObjects = new List<SpeckleObject>(); // reset the chunkness
                    }
                }
                catch (Exception e)
                {
                    failedToConvert++;
                    errors.Add(new SpeckleError { Message = $"Failed to convert element", Details = $"Element handle: {handle}" });

                    //NotifyUi("update-client", JsonConvert.SerializeObject(new
                    //{
                    //  _id = (string)client._id,
                    //  errors = "Failed to convert " + failedConvert + " objects."
                    //}));
                }
            }

            if (errors.Any())
            {
                if (failedToConvert > 0)
                    errorMsg += string.Format("Failed to convert {0} objects ",
                      failedToConvert,
                      failedToConvert == 1 ? "" : "s");
                else
                    errorMsg += string.Format("There {0} {1} error{2} ",
                     errors.Count() == 1 ? "is" : "are",
                     errors.Count(),
                     errors.Count() == 1 ? "" : "s");
            }

            var myStream = new SpeckleStream() { Objects = placeholders };

            var baseProps = new Dictionary<string, object>();

            baseProps["units"] = units;

            //baseProps["unitsDictionary"] = GetAndClearUnitDictionary();

            myStream.BaseProperties =  baseProps;

            NotifyUi("update-client", JsonConvert.SerializeObject(new
            {
                _id = (string)client._id,
                loading = true,
                isLoadingIndeterminate = true,
                loadingBlurb = "Updating stream."
            }));

            apiClient.Stream = myStream;
            var response = apiClient.StreamUpdateAsync((string)client.streamId, myStream).Result;

            var plural = objects.Count() == 1 ? "" : "s";
            NotifyUi("update-client", JsonConvert.SerializeObject(new
            {
                _id = (string)client._id,
                loading = false,
                loadingBlurb = "",
                message = $"Done sending {objects.Count()} object{plural}.",
                errorMsg,
                errors
            }));
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
                if (dataPipeClient == null) return objects;

                if (filter.Name == "Selection")
                {
                    var selFilter = filter as ElementsSelectionFilter;
                    selectionIds = selFilter.Selection.Select(x => long.Parse(x)).ToList();
                }
                else if (filter.Name == "Object Type")
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
                            case Constants.Arc:
                                request = new Request
                                {
                                    Operation = Operation.GetAllArcIds,
                                    Data = string.Empty
                                };

                                response = dataPipeClient.SendRequest(request);
                                if (!string.IsNullOrEmpty(response.Data))
                                {
                                    selectionIds.AddRange(JsonConvert.DeserializeObject<List<long>>(response.Data));
                                }
                                break;
                            case Constants.Polyline:
                                request = new Request
                                {
                                    Operation = Operation.GetAllPolylineIds,
                                    Data = string.Empty
                                };

                                response = dataPipeClient.SendRequest(request);
                                if (!string.IsNullOrEmpty(response.Data))
                                {
                                    selectionIds.AddRange(JsonConvert.DeserializeObject<List<long>>(response.Data));
                                }
                                break;
                            case Constants.Alignment:
                                request = new Request
                                {
                                    Operation = Operation.GetAllAlignmentProfileIds,
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

                // Persist state and clients to autocad file
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

        public void OnAutocadSelectionChanged()
        {
            Request request;
            Response response;
            var selectedObjectsCount = 0;

            try
            {
                if (dataPipeClient == null)
                {
                    return;
                }

                request = new Request
                {
                    Operation = Operation.GetSelectionCount,
                    Data = string.Empty
                };

                
                response = dataPipeClient.SendRequest(request);
                if (!string.IsNullOrEmpty(response.Data))
                {
                    selectedObjectsCount = JsonConvert.DeserializeObject<int>(response.Data);
                }

                NotifyUi("update-selection-count", JsonConvert.SerializeObject(new
                {
                    selectedObjectsCount
                }));
            }
            catch
            {

            }
        }

        public override void AddSelectionToSender(string args)
        {
            var selectedObjects = new List<string>();
            var request = new Request
            {
                Operation = Operation.GetSelectedIds,
                Data = string.Empty
            };

            var response = dataPipeClient.SendRequest(request);
            if (!string.IsNullOrEmpty(response.Data))
            {
                selectedObjects = JsonConvert.DeserializeObject<List<string>>(response.Data);
            }

            NotifyUi("update-selection", JsonConvert.SerializeObject(new
            {
                selectedObjects
            }));
        }

        public override void UpdateSender(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);
            var index = clients.FindIndex(cl => (string)cl._id == (string)client._id);
            clients[index] = client;

            var speckleStream = speckleStreams.FirstOrDefault(st => st.StreamId == (string)client.streamId);
            speckleStream.Name = (string)client.name;

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
            GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());
        }
    }
}
