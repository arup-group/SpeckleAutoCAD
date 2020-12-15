using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleAutoCAD
{
    public enum Operation
    {
        AddObjectsToSender = 1,
        AddReceiver,
        AddSelectionToSender,
        AddSender,
        BakeReceiver,
        GetApplicationHostName,
        GetDocumentId,
        GetDocumentLocation,
        GetFileClients,
        GetFileName,
        GetSelectionFilters,
        PushSender,
        RemoveClient,
        RemoveObjectsFromSender,
        RemoveSelectionFromSender,
        SelectClientObjects,
        UpdateSender,
        LoadStreamState,
        SaveStreamState,
        LoadClientState,
        SaveClientState

    }

    public static class Constants 
    {
        public const string SpeckleAutoCADStreamsKey = "SpeckleAutoCADStreams";
    }
    
}
