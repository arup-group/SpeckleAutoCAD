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

namespace SpeckleAutoCAD
{
    public class RequestProcessor
    {
        public RequestProcessor()
        {
            pr = new ProgressReporter();
        }

        public Document CurrentDocument => Application.DocumentManager.MdiActiveDocument;
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
                        pr.ReportProgress(() =>
                        {
                            response.Data = GetObjectAsJSON(Convert.ToInt64(request.Data));
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.GetFileName:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = CurrentDocument.Name;
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.LoadClientState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = SpeckleStateManager.ReadState(CurrentDocument, Constants.SpeckleAutoCADClientsKey);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.SaveClientState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            SpeckleStateManager.WriteState(CurrentDocument, Constants.SpeckleAutoCADClientsKey, request.Data);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.GetDocumentLocation:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = CurrentDocument.Database.Filename;
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.LoadStreamState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = SpeckleStateManager.ReadState(CurrentDocument, Constants.SpeckleAutoCADStreamsKey);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.SaveStreamState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            SpeckleStateManager.WriteState(CurrentDocument, Constants.SpeckleAutoCADStreamsKey, request.Data);
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
            var db = CurrentDocument.Database;
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
            var db = CurrentDocument.Database;
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
            var db = CurrentDocument.Database;
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
            var db = CurrentDocument.Database;
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

        private DTO.DTO GetPolylineDTO(DBObject obj)
        {
            var dto = new SpeckleAutoCAD.DTO.DTO();
            var pline = obj as Polyline;
            var payload = pline.ToPolylinePayload();
            payload.PropertySets = GetPropertySets(pline);
            dto.ObjectType = Constants.Polyline;
            dto.Data = JsonConvert.SerializeObject(payload);
            return dto;
        }

        private string GetObjectAsJSON(long longHandle)
        {
            SpeckleAutoCAD.DTO.DTO dto;
            var db = CurrentDocument.Database;
            Handle handle = new Handle(longHandle);
            ObjectId objectId = db.GetObjectId(false, handle, 0);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (DBObject obj = tr.GetObject(objectId, OpenMode.ForRead))
                {
                    if (objectId.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(Line))))
                    {
                        dto = GetLineDTO(obj);
                    }
                    else if (objectId.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(Arc))))
                    {
                        dto = GetArcDTO(obj);
                    }
                    else if (objectId.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(Polyline))))
                    {
                        dto = GetPolylineDTO(obj);
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

        private ProgressReporter pr;
    }
}
