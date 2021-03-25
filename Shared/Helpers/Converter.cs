using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace SpeckleAutoCAD.Helpers
{
    public static class Converter
    {
        public static DTO.ArcPayload ToArcPayload(this Arc arc)
        {
            //Choose the x-axis such that it lies along the line connecting the center of the arc to it's  start point (A).
            //This way start angle is always 0 as Speckle connectors expect.
            var xAxis = Normalize(arc.Center.GetVectorTo(arc.StartPoint));

            //Rotate point A by 90 degrees about z to get a point on the y-axis
            var pointB = arc.StartPoint.RotateBy(90 * System.Math.PI / 180, arc.Normal, arc.Center);
            var yAxis = Normalize(arc.Center.GetVectorTo(pointB));

            var arcNormal = Normalize(arc.Normal);
            var normalPayload = new DTO.VectorPayload
            {
                Value = new List<double> { arcNormal.X, arcNormal.Y, arcNormal.Z }
            };

            var planePayload = new DTO.PlanePayload
            {
                 Normal = normalPayload,
                 Origin = new DTO.PointPayload 
                 { 
                     Value = new List<double> {arc.Center.X, arc.Center.Y, arc.Center.Z} 
                 },
                 XDir = new DTO.VectorPayload
                 {
                     Value = new List<double> { xAxis.X, xAxis.Y, xAxis.Z }
                 },
                YDir = new DTO.VectorPayload
                {
                    Value = new List<double> { yAxis.X, yAxis.Y, yAxis.Z }
                },
            };


            var arcPayload = new DTO.ArcPayload()
            {
                Plane = planePayload,
                Radius = arc.Radius,
                StartAngle = 0,
                EndAngle = arc.TotalAngle,      //EndAngle is also the sweep because start angle is always 0
                AngleRadians = arc.TotalAngle   //Sweep              
            };

            return arcPayload;
        }

        static Vector3d Normalize(Vector3d vector)
        {
            var n = vector.GetNormal();
            var x = Math.Abs(n.X) < Tolerance.Global.EqualVector ? 0d : n.X;
            var y = Math.Abs(n.Y) < Tolerance.Global.EqualVector ? 0d : n.Y;
            var z = Math.Abs(n.Z) < Tolerance.Global.EqualVector ? 0d : n.Z;

            return new Vector3d(x, y, z);
        }
    }
}
