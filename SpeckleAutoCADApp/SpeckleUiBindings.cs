﻿using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;
using Newtonsoft.Json;
using SpeckleCore;
using System;

namespace SpeckleAutoCADApp.UI
{
    public partial class SpeckleUIBindingsAutoCAD : SpeckleUIBindings
    {
        public SpeckleUIBindingsAutoCAD(DataPipeClient dataPipeClient) : base()
        {
            this.dataPipeClient = dataPipeClient;
        }

        public override void AddObjectsToSender(string args)
        {
            
        }


        public override string GetApplicationHostName()
        {
            return "AutoCAD";
        }

        public override string GetDocumentId()
        {
            return GetDocHash();
        }

        public override string GetDocumentLocation()
        {
            var request = new Request
            {
                Operation = Operation.GetFileName,
                Data = string.Empty
            };

            if (dataPipeClient != null)
            {
                var response = dataPipeClient.SendRequest(request);
                return response.Data;
            }
            else
            {
                return string.Empty;
            }
        }

        public override string GetFileClients()
        {
            try
            {
                clients = AutocadDataService.GetClients();
                speckleStreams = AutocadDataService.GetStreams();
                return JsonConvert.SerializeObject(clients);
            }
            catch
            {
                clients = new List<dynamic>();
                speckleStreams = new List<SpeckleStream>();
                return JsonConvert.SerializeObject(clients);
            }

        }

        public override string GetFileName()
        {
            if (dataPipeClient != null)
            {
                var request = new Request
                {
                    Operation = Operation.GetFileName,
                    Data = string.Empty
                };

                var response = dataPipeClient.SendRequest(request);
                if (!string.IsNullOrEmpty(response.Data))
                {
                    return System.IO.Path.GetFileName(response.Data);
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return string.Empty;
            }

        }

        public override List<ISelectionFilter> GetSelectionFilters()
        {
            return new List<ISelectionFilter>()
            {
                new ElementsSelectionFilter
                {
                    Name = "Selection",
                    Icon = "mouse",
                    Selection = new List<string>()
                },
                new ListSelectionFilter
                {
                    Name = "Object Type",
                    Icon = "category",
                    Values = new List<string>() { Constants.Alignment, Constants.Arc, Constants.Line, Constants.Polyline }
                },
            };
        }

        //public override void PushSender(string args)
        //{

        //}

        /// <summary>
        /// Deletes a client, and persists the information to the file.
        /// </summary>
        /// <param name="args"></param>
        public override void RemoveClient(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);
            var clientIndex = clients.FindIndex(cl => cl.clientId == client.clientId);

            if (clientIndex == -1)
            {
                return;
            }

            clients.RemoveAt(clientIndex);

            var streamIndex = speckleStreams.FindIndex(x => x.StreamId == (string)client.streamId);
            if (streamIndex != -1)
            {
                speckleStreams.RemoveAt(streamIndex);
            }

            // persist the changes please
            var request = new Request
            {
                Operation = Operation.SaveClientState,
                Data = JsonConvert.SerializeObject(clients)
            };

            var response = dataPipeClient.SendRequest(request);
            request.Operation = Operation.SaveStreamState;
            request.Data = JsonConvert.SerializeObject(speckleStreams);
            response = dataPipeClient.SendRequest(request);
        }

        public override void RemoveObjectsFromSender(string args)
        {
            
        }

        public override void RemoveSelectionFromSender(string args)
        {
            
        }

        public override void SelectClientObjects(string args)
        {
            
        }

        private string GetDocHash()
        {
            return SpeckleCore.Converter.getMd5Hash(GetDocumentLocation() + GetFileName());
        }

        private DataPipeClient dataPipeClient;
        private List<dynamic> clients;
        private List<SpeckleStream> speckleStreams;
    }
}
