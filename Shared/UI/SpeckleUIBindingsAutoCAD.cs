using System;
using System.Collections.Generic;
using System.Text;
using SpeckleUiBase;
using Autodesk.Civil.ApplicationServices;

namespace SpeckleAutoCAD.UI
{
    public class SpeckleUIBindingsAutoCAD : SpeckleUIBindings
    {
        public SpeckleUIBindingsAutoCAD(CivilDocument doc)
        {

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
            return string.Empty;
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
            return string.Empty;
        }

        public override string GetFileName()
        {
            return string.Empty;
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
    }
}
