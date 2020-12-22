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

namespace SpeckleAutoCAD
{
    public class RequestProcessor
    {
        public RequestProcessor(Document document, CivilDocument civilDocument)
        {
            pr = new ProgressReporter();
            this.document = document;
            this.civilDocument = civilDocument;
        }

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
                            response.Data = document.Name;
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.LoadClientState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = SpeckleStateManager.ReadState(document, Constants.SpeckleAutoCADClientsKey);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.SaveClientState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            SpeckleStateManager.WriteState(document, Constants.SpeckleAutoCADClientsKey, request.Data);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.GetDocumentLocation:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = document.Database.Filename;
                        });

                        response.StatusCode = 200;
                        break;
                    case Operation.LoadStreamState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            response.Data = SpeckleStateManager.ReadState(document, Constants.SpeckleAutoCADStreamsKey);
                        });
                        response.StatusCode = 200;
                        break;
                    case Operation.SaveStreamState:
                        response.Operation = request.Operation;
                        pr.ReportProgress(() =>
                        {
                            SpeckleStateManager.WriteState(document, Constants.SpeckleAutoCADStreamsKey, request.Data);
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
            var db = document.Database;
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

        private string GetLinesAsString()
        {
            var lineList = new List<List<double>>();
            RXClass rxClass = RXClass.GetClass(typeof(Line));
            var db = document.Database;
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

        private string GetObjectAsJSON(long longHandle)
        {
            var dto = new SpeckleAutoCAD.DTO.DTO();
            var db = document.Database;
            Handle handle = new Handle(longHandle);
            ObjectId objectId = db.GetObjectId(false, handle, 0);

            using (var tr = db.TransactionManager.StartTransaction())
            {
                using (DBObject obj = tr.GetObject(objectId, OpenMode.ForRead))
                {
                    if (objectId.ObjectClass.IsDerivedFrom(RXClass.GetClass(typeof(Line))))
                    {
                        var acadLine = obj as Line;
                        var o = new
                        {
                            Value = new List<double>
                                {
                                    acadLine.StartPoint.X,
                                    acadLine.StartPoint.Y,
                                    acadLine.StartPoint.Z,
                                    acadLine.EndPoint.X,
                                    acadLine.EndPoint.Y,
                                    acadLine.EndPoint.Z
                                }
                        };

                        dto.ObjectType = Constants.Line;
                        dto.Data = JsonConvert.SerializeObject(o);
                    }
                    else
                    {
                        dto.ObjectType = Constants.None;
                        dto.Data = string.Empty;
                    }
                }

                tr.Commit();
            }

            return JsonConvert.SerializeObject(dto);
        }

        private ProgressReporter pr;
        private Document document;
        private CivilDocument civilDocument;
    }
}
