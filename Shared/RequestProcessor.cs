using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SpeckleAutoCAD.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System.Linq;
using Autodesk.AutoCAD.Runtime;
using SpeckleAutoCAD;
using Autodesk.Aec.PropertyData.DatabaseServices;
using ACD = Autodesk.Civil.DatabaseServices;
using System.Threading;
using Autodesk.AutoCAD.Geometry;

namespace SpeckleAutoCAD
{
    public class RequestProcessor
    {
        public RequestProcessor(EventWaitHandle requestWaitingSignal, EventWaitHandle requestCompletedSignal)
        {
            pr = new ProgressReporter();
            this.requestWaitingSignal = requestWaitingSignal;
            this.requestCompletedSignal = requestCompletedSignal;

        }

        public Action ActionToComplete 
        {
            get
            {
                return waitingAction;
            }
        }

        public int ProcessingMode
        {
            get
            {
                return processingMode;
            }
            set
            {
                lock(processingModeLock) 
                {
                    processingMode = value;
                }
            }
        }
        public Document BoundDocument { get; set; }
        public CivilDocument BoundCivilDocument { get; set; }
        public string ProcessRequest(string sRequest)
        {
            Response response;

            try
            {
                response = new Response();
                var request = JsonConvert.DeserializeObject<Request>(sRequest);
                switch (request.Operation)
                {
                    case Operation.GetAllLines:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetLinesAsString();
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.GetAllLineIds:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetLineIdsAsJSON();
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.GetAllArcIds:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetArcIdsAsJSON();
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.GetAllPolylineIds:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetPolylineIdsAsJSON();
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.GetObject:
                        response.Operation = request.Operation;
                        Action a = () =>
                        {
                            response.Data = GetObjectAsJSON(Convert.ToInt64(request.Data));
                        };

                        if (ProcessingMode == 1)
                        {
                            waitingAction = a;
                            requestWaitingSignal.Set();
                            requestCompletedSignal.WaitOne(-1);
                        }
                        else
                        {
                            waitingAction = null;
                            pr.ReportProgress(a);
                        }

                        response.StatusCode = 200;
                        break;
                    case Operation.GetFileName:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = BoundDocument.Name;
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.LoadClientState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = SpeckleStateManager.ReadState(BoundDocument, Constants.SpeckleAutoCADClientsKey);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.SaveClientState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            SpeckleStateManager.WriteState(BoundDocument, Constants.SpeckleAutoCADClientsKey, request.Data);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.GetDocumentLocation:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = BoundDocument.Database.Filename;
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.LoadStreamState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = SpeckleStateManager.ReadState(BoundDocument, Constants.SpeckleAutoCADStreamsKey);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.SaveStreamState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            SpeckleStateManager.WriteState(BoundDocument, Constants.SpeckleAutoCADStreamsKey, request.Data);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.GetSelectionCount:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetSelectionCountAsJSON();
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.GetSelectedIds:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetSelectedIdsAsJSON();
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.GetAllAlignmentIds:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetObjectHandlesAsJSON(typeof(ACD.Alignment));
                        });

                        response.StatusCode = 200;
                        break;
                    default:
                        response.Data = string.Empty;
                        response.StatusCode = 400;
                        break;
                }
            }
            catch (System.Exception ex)
            {
                response = new Response
                {
                    Data = ex.Message.ToString(),
                    StatusCode = 500
                };
            }
            
            var sResponse = JsonConvert.SerializeObject(response);
            return sResponse;
        }

        private string GetLineIdsAsJSON()
        {
            var lineList = new List<long>();
            RXClass rxClass = RXClass.GetClass(typeof(Line));
            var db = BoundDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                var lineIds =
                    from ObjectId id in btr
                    where id.ObjectClass.IsDerivedFrom(rxClass)
                    select id;

                foreach (var id in lineIds)
                {
                    using (var line = (Line)tr.GetObject(id, OpenMode.ForRead))
                    {
                        lineList.Add(line.Handle.Value);
                    }
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(lineList);
        }

        private string GetArcIdsAsJSON()
        {
            var arcList = new List<long>();
            RXClass rxClass = RXClass.GetClass(typeof(Arc));
            var db = BoundDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                var arcIds =
                    from ObjectId id in btr
                    where id.ObjectClass.IsDerivedFrom(rxClass)
                    select id;

                foreach (var id in arcIds)
                {
                    using (var arc = (Arc)tr.GetObject(id, OpenMode.ForRead))
                    {
                        arcList.Add(arc.Handle.Value);
                    }
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(arcList);
        }

        private string GetPolylineIdsAsJSON()
        {
            var pList = new List<long>();
            RXClass rxClass = RXClass.GetClass(typeof(Polyline));
            var db = BoundDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                var ids =
                    from ObjectId id in btr
                    where id.ObjectClass.IsDerivedFrom(rxClass)
                    select id;

                foreach (var id in ids)
                {
                    using (var pLine = (Polyline)tr.GetObject(id, OpenMode.ForRead))
                    {
                        pList.Add(pLine.Handle.Value);
                    }
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(pList);
        }

        private string GetLinesAsString()
        {
            var lineList = new List<List<double>>();
            RXClass rxClass = RXClass.GetClass(typeof(Line));
            var db = BoundDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                var lineIds = 
                    from ObjectId id in btr
                    where id.ObjectClass.IsDerivedFrom(rxClass)
                    select id;

                foreach (var id in lineIds)
                {
                    using (var line = (Line)tr.GetObject(id, OpenMode.ForRead))
                    {
                        var coordinates = new List<double>()
                        {
                                line.StartPoint.X,
                                line.StartPoint.Y,
                                line.StartPoint.Z,
                                line.EndPoint.X,
                                line.EndPoint.Y,
                                line.EndPoint.Z
                        };

                        lineList.Add(coordinates);
                    }
                }
                        
                tr.Commit();
            }

            return JsonConvert.SerializeObject(lineList);
        }

        private DTO.DTO GetLineDTO(DBObject obj)
        {
            var dto = new SpeckleAutoCAD.DTO.DTO();
            var line = obj as Line;
            var o = line.ToLinePayload();
            o.PropertySets = GetPropertySets(line);
            dto.ObjectType = Constants.Line;
            dto.Data = JsonConvert.SerializeObject(o);

            return dto;
        }

        private DTO.DTO GetArcDTO(DBObject obj)
        {
            var dto = new SpeckleAutoCAD.DTO.DTO();
            var arc = obj as Arc;
            var arcPayload = arc.ToArcPayload();
            arcPayload.PropertySets = GetPropertySets(arc);
            dto.ObjectType = Constants.Arc;
            dto.Data = JsonConvert.SerializeObject(arcPayload);
            return dto;
        }

        private DTO.DTO GetDTO(Polyline polyline)
        {
            var dto = new SpeckleAutoCAD.DTO.DTO();
            var payload = polyline.ToPolycurvePayload();
            payload.PropertySets = GetPropertySets(polyline);
            dto.ObjectType = Constants.Polyline;
            dto.Data = JsonConvert.SerializeObject(payload);
            return dto;
        }

        private DTO.DTO GetDTO(Polyline3d polyline)
        {
            var dto = new SpeckleAutoCAD.DTO.DTO();
            var payload = polyline.ToPolylinePayload();
            payload.PropertySets = GetPropertySets(polyline);
            dto.ObjectType = Constants.Polyline3d;
            dto.Data = JsonConvert.SerializeObject(payload);
            return dto;
        }

        private string GetObjectAsJSON(long longHandle)
        {
            SpeckleAutoCAD.DTO.DTO dto;
            var db = BoundDocument.Database;
            Handle handle = new Handle(longHandle);
            ObjectId objectId = db.GetObjectId(false, handle, 0);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (DBObject obj = tr.GetObject(objectId, OpenMode.ForRead))
                {
                    if (objectId.ObjectClass.GetRuntimeType() == typeof(Line))
                    {
                        dto = GetLineDTO(obj);
                    }
                    else if (objectId.ObjectClass.GetRuntimeType() == typeof(Arc))
                    {
                        dto = GetArcDTO(obj);
                    }
                    else if (objectId.ObjectClass.GetRuntimeType() == typeof(Polyline))
                    {
                        dto = GetDTO(obj as Polyline);
                    }
                    else if (objectId.ObjectClass.GetRuntimeType() == typeof(ACD.Alignment))
                    {
                        //var x = obj as ACD.Alignment;
                        //var y = x.GetPolyline();
                        dto = GetAlignmentDTO(obj);
                        //dto = new DTO.DTO();
                    }
                    else
                    {
                        dto = new DTO.DTO();
                        dto.ObjectType = Constants.None;
                        dto.Data = string.Empty;
                    }
                    
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(dto);
        }

        private Dictionary<string, Dictionary<string, object>> GetPropertySets(DBObject o)
        {
            var propertySetsDTO = new Dictionary<string, Dictionary<string, object>>();
            var propertySetIds = PropertyDataServices.GetPropertySets(o);

            foreach (ObjectId propertySetId in propertySetIds)
            {
                using (Transaction transProp = HostApplicationServices.WorkingDatabase.TransactionManager.StartTransaction())
                {
                    using (PropertySet propertySet = transProp.GetObject(propertySetId, OpenMode.ForWrite) as PropertySet)
                    {
                        var propertySetDTO = new Dictionary<string, object>();
                        propertySetsDTO.Add(propertySet.PropertySetDefinitionName, propertySetDTO);
                        foreach (PropertySetData propertySetData in propertySet.PropertySetData)
                        {
                            propertySetDTO.Add(propertySet.PropertyIdToName(propertySetData.Id), propertySetData.GetData());
                        }
                    }
                }
            }

            return propertySetsDTO;
        }

        private string GetSelectionCountAsJSON()
        {
            int selectionCount = 0;
            var editor = BoundDocument.Editor;
            var selectionResult = editor.SelectImplied();
            if (selectionResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
            {
                selectionCount = selectionResult.Value.Count;
            }

            return JsonConvert.SerializeObject(selectionCount);
        }

        private string GetSelectedIdsAsJSON()
        {
            var list = new List<string>();
            var db = BoundDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var editor = BoundDocument.Editor;
                var selectionResult = editor.SelectImplied();
                if (selectionResult.Status == Autodesk.AutoCAD.EditorInput.PromptStatus.OK)
                {
                    foreach (var id in selectionResult.Value.GetObjectIds())
                    {
                        using (var o = tr.GetObject(id, OpenMode.ForRead))
                        {
                            list.Add(o.Handle.Value.ToString());
                        }
                    }
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(list);
        }

        private string GetObjectHandlesAsJSON(Type t)
        {
            var handles = new List<long>();
            var db = BoundDocument.Database;
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var btr = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);
                var ids =
                    from ObjectId id in btr
                    where id.ObjectClass.GetRuntimeType() == t
                    select id;

                foreach (var id in ids)
                {
                    using (var o = tr.GetObject(id, OpenMode.ForRead))
                    {
                        handles.Add(o.Handle.Value);
                    }
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(handles);
        }

        public static Point3dCollection Get3dPath(Polyline polyline, ACD.Profile profile)
        {
            Point3d point;
            double interval = 1.0;
            var points = new Point3dCollection();
            var station = -interval;
            double elevation;

            do
            {
                station += interval;
                if (station > polyline.Length)
                {
                    station = polyline.Length;
                }

                elevation = profile.ElevationAt(station);
                point = polyline.GetPointAtDist(station);
                point = new Point3d(point.X, point.Y, elevation);
                points.Add(point);
            } while (station < polyline.Length);

            return points;
        }


        private DTO.DTO GetAlignmentDTO(DBObject obj)
        {
            using (var tr = obj.Database.TransactionManager.StartTransaction())
            {
                string profileName = string.Empty;
                Point3dCollection points = null;
                var dto = new SpeckleAutoCAD.DTO.DTO();
                var alignment = obj as ACD.Alignment;
                var polyLineId = alignment.GetPolyline();
                var polyline = polyLineId.GetObject(OpenMode.ForRead) as Polyline;

                var profileIds = alignment.GetProfileIds();
                foreach (ObjectId profileId in profileIds)
                {
                    using (var profile = tr.GetObject(profileId, OpenMode.ForRead) as ACD.Profile)
                    {
                        if (profile.ProfileType == ACD.ProfileType.EG)
                        {
                            continue;
                        }

                        profileName = profile.Name;
                        points = Get3dPath(polyline, profile);
                        break;
                    }
                }

                var poly3d = new Polyline3d(Poly3dType.SimplePoly, points, polyline.Closed);
                var payload = poly3d.ToPolylinePayload();
                payload.Name = $"{alignment.Name} - {profileName}";
                var properties = payload.Properties;

                properties["AlignmentName"] = alignment.Name;
                properties["ProfileName"] = profileName;


                payload.PropertySets = GetPropertySets(alignment);
                dto.ObjectType = Constants.Polyline3d;
                dto.Data = JsonConvert.SerializeObject(payload);
                return dto;
            }
        }

        private ProgressReporter pr;
        private int processingMode;
        private object processingModeLock = new object();
        private EventWaitHandle requestWaitingSignal;
        private EventWaitHandle requestCompletedSignal;
        private Action waitingAction;
    }
}
