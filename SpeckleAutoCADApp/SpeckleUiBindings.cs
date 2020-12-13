using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;

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

        public override void AddSender(string args)
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
            return string.Empty;
        }

        public override string GetDocumentLocation()
        {
            return string.Empty;
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
            return new List<ISelectionFilter>();
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

        private DataPipeClient dataPipeClient;
    }
}
