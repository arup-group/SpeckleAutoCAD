using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;
using Newtonsoft.Json;
using SpeckleCore;

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

        public override void AddReceiver(string args)
        {
            
        }

        public override void AddSelectionToSender(string args)
        {
            
        }



        public override void BakeReceiver(string args)
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

            var response = dataPipeClient.SendRequest(request);
            return response.Data;
        }

        public override string GetFileClients()
        {
            var request = new Request
            {
                Operation = Operation.GetFileClients,
                Data = string.Empty
            };

            var response = dataPipeClient.SendRequest(request);
            return response.Data;
        }

        public override string GetFileName()
        {
            var request = new Request
            {
                Operation = Operation.GetFileName,
                Data = string.Empty
            };

            var response = dataPipeClient.SendRequest(request);
            return response.Data;
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
                    Values = new List<string>() { "Arc", "Line", "Polyline" }
                },
            };
        }

        public override void PushSender(string args)
        {
            
        }

        public override void RemoveClient(string args)
        {
            
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

        public override void UpdateSender(string args)
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
