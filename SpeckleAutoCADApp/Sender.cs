using SpeckleUiBase;
using System.Collections.Generic;
using SpeckleAutoCAD;
using Newtonsoft.Json;
using SpeckleCore;
using System.Reflection;
using System;

namespace SpeckleAutoCADApp.UI
{
    public partial class SpeckleUIBindingsAutoCAD
    {
        public override void AddSender(string args)
        {
            var client = JsonConvert.DeserializeObject<dynamic>(args);
            clients.Add(client);
            var speckleStream = new SpeckleStream() { StreamId = (string)client.streamId, Objects = new List<SpeckleObject>() };
            speckleStreams.Add(speckleStream);

            var request = new Request
            {
                Operation = Operation.SaveClientState,
                Data = JsonConvert.SerializeObject(clients)
            };
            var response = dataPipeClient.SendRequest(request);

            request.Operation = Operation.SaveStreamState;
            request.Data = JsonConvert.SerializeObject(speckleStreams);
            response = dataPipeClient.SendRequest(request);

            ISelectionFilter filter = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(client.filter), GetFilterType(client.filter.Type.ToString()));
            //GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());
            GetSelectionFilterObjects(filter, client._id.ToString(), client.streamId.ToString());
        }

        private Type GetFilterType(string typeString)
        {
            Assembly ass = typeof(ISelectionFilter).Assembly;
            return ass.GetType(typeString);
        }

        private IEnumerable<SpeckleObject> GetSelectionFilterObjects(ISelectionFilter filter, string clientId, string streamId)
        {
            //var doc = CurrentDoc.Document;
            IEnumerable<SpeckleObject> objects = new List<SpeckleObject>();

            //var selectionIds = new List<string>();

            //if (filter.Name == "Selection")
            //{
            //    var selFilter = filter as ElementsSelectionFilter;
            //    selectionIds = selFilter.Selection;
            //}
            //else if (filter.Name == "Category")
            //{
            //    var catFilter = filter as ListSelectionFilter;
            //    var bics = new List<BuiltInCategory>();
            //    var categories = Globals.GetCategories(doc);
            //    IList<ElementFilter> elementFilters = new List<ElementFilter>();

            //    foreach (var cat in catFilter.Selection)
            //    {
            //        elementFilters.Add(new ElementCategoryFilter(categories[cat].Id));
            //    }
            //    LogicalOrFilter categoryFilter = new LogicalOrFilter(elementFilters);

            //    selectionIds = new FilteredElementCollector(doc)
            //      .WhereElementIsNotElementType()
            //      .WhereElementIsViewIndependent()
            //      .WherePasses(categoryFilter)
            //      .Select(x => x.UniqueId).ToList();

            //}
            //else if (filter.Name == "View")
            //{
            //    var viewFilter = filter as ListSelectionFilter;

            //    var views = new FilteredElementCollector(doc)
            //      .WhereElementIsNotElementType()
            //      .OfClass(typeof(View))
            //      .Where(x => viewFilter.Selection.Contains(x.Name));

            //    foreach (var view in views)
            //    {
            //        var ids = new FilteredElementCollector(doc, view.Id)
            //        .WhereElementIsNotElementType()
            //        .WhereElementIsViewIndependent()
            //        .Where(x => x.IsPhysicalElement())
            //        .Select(x => x.UniqueId).ToList();

            //        selectionIds = selectionIds.Union(ids).ToList();
            //    }
            //}
            //else if (filter.Name == "Parameter")
            //{
            //    try
            //    {
            //        var propFilter = filter as PropertySelectionFilter;
            //        var query = new FilteredElementCollector(doc)
            //          .WhereElementIsNotElementType()
            //          .WhereElementIsNotElementType()
            //          .WhereElementIsViewIndependent()
            //          .Where(x => x.IsPhysicalElement())
            //          .Where(fi => fi.LookupParameter(propFilter.PropertyName) != null);

            //        propFilter.PropertyValue = propFilter.PropertyValue.ToLowerInvariant();

            //        switch (propFilter.PropertyOperator)
            //        {
            //            case "equals":
            //                query = query.Where(fi => GetStringValue(fi.LookupParameter(propFilter.PropertyName)) == propFilter.PropertyValue);
            //                break;
            //            case "contains":
            //                query = query.Where(fi => GetStringValue(fi.LookupParameter(propFilter.PropertyName)).Contains(propFilter.PropertyValue));
            //                break;
            //            case "is greater than":
            //                query = query.Where(fi => UnitUtils.ConvertFromInternalUnits(
            //                  fi.LookupParameter(propFilter.PropertyName).AsDouble(),
            //                  fi.LookupParameter(propFilter.PropertyName).DisplayUnitType) > double.Parse(propFilter.PropertyValue));
            //                break;
            //            case "is less than":
            //                query = query.Where(fi => UnitUtils.ConvertFromInternalUnits(
            //                 fi.LookupParameter(propFilter.PropertyName).AsDouble(),
            //                 fi.LookupParameter(propFilter.PropertyName).DisplayUnitType) < double.Parse(propFilter.PropertyValue));
            //                break;
            //            default:
            //                break;
            //        }

            //        selectionIds = query.Select(x => x.UniqueId).ToList();

            //    }
            //    catch (Exception e)
            //    {
            //        Console.WriteLine(e);
            //    }
            //}

            //// LOCAL STATE management
            //objects = selectionIds.Select(id =>
            //{
            //    var temp = new SpeckleObject();
            //    temp.Properties["revitUniqueId"] = id;
            //    temp.Properties["__type"] = "Sent Object";
            //    return temp;
            //});


            //var myStream = LocalState.FirstOrDefault(st => st.StreamId == streamId);

            //myStream.Objects.Clear();
            //myStream.Objects.AddRange(objects);

            //var myClient = ClientListWrapper.clients.FirstOrDefault(cl => (string)cl._id == (string)clientId);
            //myClient.objects = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(myStream.Objects));

            //// Persist state and clients to revit file
            //Queue.Add(new Action(() =>
            //{
            //    using (Transaction t = new Transaction(CurrentDoc.Document, "Update local storage"))
            //    {
            //        t.Start();
            //        SpeckleStateManager.WriteState(CurrentDoc.Document, LocalState);
            //        SpeckleClientsStorageManager.WriteClients(CurrentDoc.Document, ClientListWrapper);
            //        t.Commit();
            //    }
            //}));
            //Executor.Raise();
            //var plural = objects.Count() == 1 ? "" : "s";
            //if (objects.Count() != 0)
            //    NotifyUi("update-client", JsonConvert.SerializeObject(new
            //    {
            //        _id = clientId,
            //        expired = true,
            //        objects = myClient.objects,
            //        //message = $"You have added {objects.Count()} object{plural} to this sender."
            //    }));

            return objects;
        }
    }
}
