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
        SaveClientState,
        GetObject,
        GetAllLines,
        GetAllLineIds,
        GetAllArcIds,
        GetAllPolylineIds,
        GetSelectionCount,
        GetSelectedIds,
        GetAllAlignmentIds,
        GetAllAlignmentProfileIds,
        GetLengthUnit

    }

    public static class Constants 
    {
        public const string SpeckleAutoCADStreamsKey = "com.arup.speckle.streams";
        public const string SpeckleAutoCADClientsKey = "com.arup.speckle.clients";
        public const string Arc = "Arc";
        public const string Line = "Line";
        public const string None = "None";
        public const string Polyline = "Polyline";
        public const string Polyline3d = "Polyline3d";
        public const string Alignment = "Alignment";
    }

    public enum SegmentType
    {
        Line = 0,
        Arc = 1,
        Coincident = 2,
        Point = 3,
        Empty = 4
    }
}
