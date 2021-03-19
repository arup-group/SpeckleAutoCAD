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
            var plane = new Plane(arc.Center, arc.Normal);
            var coordinateSystem = plane.GetCoordinateSystem();

            var normalPayload = new DTO.VectorPayload
            {
                Value = new List<double> { plane.Normal.X, plane.Normal.Y, plane.Normal.Z }
            };

            var planePayload = new DTO.PlanePayload
            {
                 Normal = normalPayload,
                 Origin = new DTO.PointPayload 
                 { 
                     Value = new List<double> {coordinateSystem.Origin.X, coordinateSystem.Origin.Y, coordinateSystem.Origin.Z} 
                 },
                 XDir = new DTO.VectorPayload
                 {
                     Value = new List<double> { coordinateSystem.Xaxis.X, coordinateSystem.Xaxis.Y, coordinateSystem.Xaxis.Z }
                 },
                YDir = new DTO.VectorPayload
                {
                    Value = new List<double> { coordinateSystem.Yaxis.X, coordinateSystem.Yaxis.Y, coordinateSystem.Yaxis.Z }
                },
            };

            var arcPayload = new DTO.ArcPayload()
            {
                Plane = planePayload,
                Radius = arc.Radius,
                StartAngle = arc.StartAngle,
                EndAngle = arc.EndAngle,
                AngleRadians = arc.TotalAngle                 
            };

            return arcPayload;
        }

    }
}
