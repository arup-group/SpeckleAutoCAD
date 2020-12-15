using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace SpeckleAutoCAD.Helpers
{
    public static class SpeckleStateManager
    {
        public static string ReadState(Document doc, string key)
        {
            // https://adndevblog.typepad.com/autocad/2012/05/how-can-i-store-my-custom-information-in-a-dwg-file.html

            var db = doc.Database;
            string state;

            try
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // Find the NOD in the database
                    DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead);

                    if (!nod.Contains(key))
                    {
                        return string.Empty;
                    }

                    ObjectId objectId = nod.GetAt(key);
                    Xrecord xRecord = (Xrecord)trans.GetObject(objectId, OpenMode.ForRead);
                    var sb = new StringBuilder();

                    foreach (TypedValue value in xRecord.Data)
                    {
                        sb.Append(value.Value.ToString());
                    }

                    state = sb.ToString();
                    trans.Commit();
                    return state;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error reading Speckle state.", ex);
            }
        }

        public static void WriteState(Document doc, string key, string state)
        {
            // https://adndevblog.typepad.com/autocad/2012/05/how-can-i-store-my-custom-information-in-a-dwg-file.html

            var db = doc.Database;
            Xrecord xRecord;

            try
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    // Find the NOD in the database
                    DBDictionary nod = (DBDictionary)trans.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForWrite);

                    if (nod.Contains(key))
                    {
                        ObjectId objectId = nod.GetAt(key);
                        xRecord = (Xrecord)trans.GetObject(objectId, OpenMode.ForWrite);
                        xRecord.Data = state.ToResultBuffer();
                    }
                    else
                    {
                        xRecord = new Xrecord();
                        xRecord.Data = state.ToResultBuffer();
                        nod.SetAt(key, xRecord);
                        trans.AddNewlyCreatedDBObject(xRecord, true);
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error writing Speckle state.", ex);
            }
        }

       
    }
}
