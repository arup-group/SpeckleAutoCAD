using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeckleAutoCAD.DTO
{
    public class DTO
    {
        public string ObjectType { get; set; }
        public string Data { get; set; }
    }

    public class Payload
    {
        public Dictionary<string, Dictionary<string, dynamic>> PropertySets { get; set; }
    }

    public class LinePayload : Payload
    {
        public List<double> Coordinates { get; set; }
    }

    public class ArcPayload : Payload
    {
        public PlanePayload Plane { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }
        public double AngleRadians { get; set; }
    }

    public class PointPayload : Payload
    {
        public List<double> Value { get; set; }
    }

    public class VectorPayload : Payload
    {
        public List<double> Value { get; set; }
    }

    public class PlanePayload : Payload
    {
        public VectorPayload Normal { get; set; }
        public PointPayload Origin { get; set; }
        public VectorPayload XDir { get; set; }
        public VectorPayload YDir { get; set; }
    }

    public class Segment
    {
        public SegmentType SegmentType { get; set; }
        public string Data { get; set; }
    }

    public class PolylinePayload : Payload
    {
        public bool Closed { get; set; }
        public List<Segment> Segments { get; set; }


    }
}
