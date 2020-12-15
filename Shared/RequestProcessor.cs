using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using SpeckleAutoCAD.Helpers;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;

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
            var response = new Response();
            var request = JsonConvert.DeserializeObject<Request>(sRequest);   
            switch (request.Operation)
            {                
                case Operation.GetFileName:
                    response.Operation = request.Operation;
                    pr.ReportProgress(() =>
                    {
                        response.Data = document.Name;
                    });

                    response.StatusCode = 200;
                    break;
                case Operation.GetFileClients:
                    response.Operation = request.Operation;
                    //pr.ReportProgress(() =>
                    //{
                    //    response.Data = document.Database.Filename;
                    //});
                    response.Data = string.Empty;
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
                default:
                    response.Data = string.Empty;
                    response.StatusCode = 400;
                    break;
            }
            
            var sResponse = JsonConvert.SerializeObject(response);
            return sResponse;
        }

        private ProgressReporter pr;
        private Document document;
        private CivilDocument civilDocument;
    }
}
