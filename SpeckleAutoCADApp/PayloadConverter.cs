using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeckleCore;
using SpeckleCoreGeometryClasses;
using Newtonsoft.Json;
using SpeckleAutoCAD;
using SpeckleAutoCAD.DTO;

namespace SpeckleAutoCADApp
{
    public static class PayloadConverter
    {
        public static SpeckleLine ToSpeckleLine(this LinePayload payload)
        {
            var line = new SpeckleLine(payload.Coordinates);
            return line;
        }

        public static SpecklePolyline ToSpecklePolyline(this PolylinePayload payload)
        {
            var line = new SpecklePolyline(payload.Coordinates);
            line.Closed = payload.Closed;
            return line;
        }

        public static SpeckleArc ToSpeckleArc(this ArcPayload payload)
        {
            var arc = new SpeckleArc(
                    payload.Plane.ToSpecklePlane(),
                    payload.Radius,
                    payload.StartAngle,
                    payload.EndAngle,
                    payload.AngleRadians);
            return arc;
        }

        public static SpecklePlane ToSpecklePlane(this PlanePayload payload)
        {
            var origin = payload.Origin.ToSpecklePoint();
            var normal = payload.Normal.ToSpeckleVector();
            var xAxis = payload.XDir.ToSpeckleVector();
            var yAxis = payload.YDir.ToSpeckleVector();
            var plane = new SpecklePlane(origin, normal, xAxis, yAxis);
            return plane;
        }

        public static SpecklePoint ToSpecklePoint(this PointPayload payload)
        {
            return new SpecklePoint
            {
                Value = payload.Value
            };
        }

        public static SpeckleVector ToSpeckleVector(this VectorPayload payload)
        {
            return new SpeckleVector
            {
                Value = payload.Value
            };
        }

        public static SpecklePolycurve ToSpecklePolycurve(this PolycurvePayload payload)
        {
            var polycurve = new SpecklePolycurve();
            polycurve.Closed = payload.Closed;
            polycurve.Segments = new List<SpeckleObject>();

            foreach (var segment in payload.Segments)
            {
                if (segment.SegmentType == SegmentType.Arc)
                {
                    var arcPayload = JsonConvert.DeserializeObject<ArcPayload>(segment.Data);
                    polycurve.Segments.Add(arcPayload.ToSpeckleArc());
                }
                else if (segment.SegmentType == SegmentType.Line)
                {
                    var linePayload = JsonConvert.DeserializeObject<LinePayload>(segment.Data);
                    polycurve.Segments.Add(linePayload.ToSpeckleLine());
                }
            }

            return polycurve;
        }
    }
}
