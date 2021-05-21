using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpeckleAutoCAD.Helpers
{
    public class AutoCadConsoleHelper
    {
        public static ObjectId PickAlignment(Editor editor, string prompt)
        {
            // Create a TypedValue array and define the filter criteria
            TypedValue[] selectionCriteria = new TypedValue[1];
            selectionCriteria[0] = new TypedValue((int)DxfCode.Start, "AECC_ALIGNMENT");

            PromptSelectionResult selectionResult = GetFilteredSingleSelection(prompt, selectionCriteria);
            if (selectionResult.Status == PromptStatus.OK)
            {
                foreach (SelectedObject o in selectionResult.Value)
                {
                    return o.ObjectId;
                }
            }
            else
            {
                editor.WriteMessage("\nNo valid object selected.");

            }

            return ObjectId.Null;
        }

        public static Autodesk.AutoCAD.EditorInput.PromptSelectionResult GetFilteredSingleSelection(String prompt, TypedValue[] criteria)
        {
            // Get the editor
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;

            SelectionFilter selectionFilter = new SelectionFilter(criteria);

            // Create selection prompt
            PromptSelectionOptions selectionOptions = new PromptSelectionOptions();
            selectionOptions.SingleOnly = true;
            selectionOptions.MessageForAdding = "\n" + prompt;

            // Get selection
            PromptSelectionResult selectionResult = ed.GetSelection(selectionOptions, selectionFilter);

            // Return selection
            return selectionResult;
        }

        public static void Explode(Autodesk.AutoCAD.DatabaseServices.Entity ent, List<Autodesk.AutoCAD.DatabaseServices.DBObject> entities)
        {
            var objs = new DBObjectCollection();

            if (ent != null)
            {
                entities.Add(ent);

                try
                {
                    ent.Explode(objs);
                }
                catch
                {
                }

                if (objs.Count == 0)
                {
                    return;
                }
                else
                {
                    foreach (Autodesk.AutoCAD.DatabaseServices.Entity obj in objs)
                    {
                        Explode(obj, entities);
                    }
                }
            }
        }

        private static void TotalDistance(Point3dCollection points)
        {
            int i = 0;
            double sum = 0;
            for (i = 0; i < points.Count - 1; i++)
            {
                var j = i + 1;
                var deltaX = Math.Abs(points[j].X - points[i].X);
                var deltaY = Math.Abs(points[j].Y - points[i].Y);
                var hyp = Math.Sqrt(deltaX * deltaX + deltaY * deltaY);
                sum += hyp;
            }
        }
        public static Polyline3d AlignmentToPolyline3d(Alignment alignment, Profile profile)
        {
            Point3d point;
            Polyline3d p3d = null;
            double interval = 1.0;
            var elevations = new Point3dCollection();
            var station = -interval;
            double elevation;
            var db = alignment.Database;
            
            using (var tr = db.TransactionManager.StartTransaction())
            {
                var polylineId = alignment.GetPolyline();
                using (var polyline = tr.GetObject(polylineId, OpenMode.ForRead) as Polyline)
                {
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
                        elevations.Add(point);
                    } while (station < polyline.Length);

                    p3d = new Polyline3d(Poly3dType.SimplePoly, elevations, polyline.Closed);
                }
            }

            TotalDistance(elevations);
            foreach (var x in p3d)
            {
                var y = x;
            }
            return p3d;

        }

        public static Polyline3d AlignmentToPolyline3d2(Alignment alignment, Profile profile)
        {
            Point3d point;
            Polyline3d p3d = null;

            var elevations = new Point3dCollection();
            var station = 0d;
            double elevation;
            var db = alignment.Database;

            using (var tr = db.TransactionManager.StartTransaction())
            {
                var polylineId = alignment.GetPolyline();
                using (var polyline = tr.GetObject(polylineId, OpenMode.ForRead) as Polyline)
                {
                    do
                    {
                        station += 0.001;
                        if (station > polyline.Length)
                        {
                            station = polyline.Length;
                        }

                        elevation = profile.ElevationAt(station);
                        point = polyline.GetPointAtDist(station);
                        point = new Point3d(point.X, point.Y, elevation);
                        elevations.Add(point);
                    } while (station < polyline.Length);

                    var fid = FeatureLine.Create("george", polylineId);
                    var featureLine = tr.GetObject(fid, OpenMode.ForWrite) as FeatureLine;
                    foreach (Point3d p in elevations)
                    {
                        featureLine.InsertElevationPoint(p);
                    }
                    var objs = new DBObjectCollection();
                    featureLine.Explode(objs);
                    p3d = new Polyline3d(Poly3dType.SimplePoly, elevations, polyline.Closed);
                }
            }

            return p3d;

        }

        public static void ExplodeProfile(Autodesk.AutoCAD.DatabaseServices.Entity ent, List<Autodesk.AutoCAD.DatabaseServices.DBObject> entities)
        {
            var objs = new DBObjectCollection();

            if (ent != null)
            {
                ent.Explode(objs);
                var block = objs[0] as Autodesk.AutoCAD.DatabaseServices.Entity;
                objs.Clear();
                block.Explode(objs);

                foreach (Autodesk.AutoCAD.DatabaseServices.Entity obj in objs)
                {
                    entities.Add(obj);
                }
            }
        }
    }
}
