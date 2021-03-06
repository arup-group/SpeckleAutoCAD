﻿using System;
using System.Collections.Generic;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Newtonsoft.Json;

namespace SpeckleAutoCAD.Helpers
{
    public static class Converter
    {
        public static DTO.PolylinePayload ToPolylinePayload(this Polyline3d line)
        {
            Point3d point;
            var coordinates = new List<double>();

            foreach (PolylineVertex3d vertex in line)
            {
                point = vertex.Position;
                coordinates.Add(point.X);
                coordinates.Add(point.Y);
                coordinates.Add(point.Z);
            }

            var o = new DTO.PolylinePayload
            {
                
                Closed = line.Closed,
                Coordinates = coordinates
            };

            o.Properties = new Dictionary<string, dynamic>();

            return o;
        }

        public static DTO.LinePayload ToLinePayload(this Line line)
        {
            var o = new DTO.LinePayload
            {
                Coordinates = new List<double>
                            {
                                line.StartPoint.X,
                                line.StartPoint.Y,
                                line.StartPoint.Z,
                                line.EndPoint.X,
                                line.EndPoint.Y,
                                line.EndPoint.Z
                             }
            };

            return o;
        }

        public static DTO.LinePayload ToLinePayload(this LineSegment3d line)
        {
            var o = new DTO.LinePayload
            {
                Coordinates = new List<double>
                            {
                                line.StartPoint.X,
                                line.StartPoint.Y,
                                line.StartPoint.Z,
                                line.EndPoint.X,
                                line.EndPoint.Y,
                                line.EndPoint.Z
                             }
            };

            return o;
        }

        public static DTO.ArcPayload ToArcPayload(this Arc arc)
        {
            //Whilst AutoCAD has a number of angle display formats, the actual angle unit is the radian.
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
                    Value = new List<double> { arc.Center.X, arc.Center.Y, arc.Center.Z }
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

        public static DTO.PolycurvePayload PointsToPolycurvePayload(List<Point3d> points, bool closed, double length, string layer)
        {
            DTO.Segment segment;
            var polycurvePayload = new DTO.PolycurvePayload();
            polycurvePayload.Closed = closed;
            polycurvePayload.Segments = new List<DTO.Segment>();

            for (int i = 0; i < points.Count - 2; i++)
            {
                segment = new DTO.Segment();
                segment.SegmentType = SegmentType.Line;
                var linePayload = new DTO.LinePayload
                {
                    Coordinates = new List<double>
                            {
                                points[i].X,
                                points[i].Y,
                                points[i].Z,
                                points[i + 1].X,
                                points[i + 1].Y,
                                points[i + 1].Z
                             }
                };

                segment.Data = JsonConvert.SerializeObject(linePayload);
                polycurvePayload.Segments.Add(segment);
            }

            var lastIndex = points.Count - 1;
            var properties = new Dictionary<string, dynamic>()
            {
                {"EndPoint", new DTO.PointPayload {Value = new List<double> {points[lastIndex].X, points[lastIndex].Y, points[lastIndex].Z} }},
                {"Layer", layer},
                {"Length", length},
                {"StartPoint", new DTO.PointPayload {Value = new List<double> {points[0].X, points[0].Y, points[0].Z} }},
            };

            polycurvePayload.Properties = properties;
            return polycurvePayload;
        }


        public static DTO.PolycurvePayload ToPolycurvePayload(this Polyline polyline)
        {
            DTO.Segment segment;
            var polycurvePayload = new DTO.PolycurvePayload();
            polycurvePayload.Closed = polyline.Closed;
            polycurvePayload.Segments = new List<DTO.Segment>();
            
            for (int i = 0; i < polyline.NumberOfVertices; i++)
            {
                var segmentType = polyline.GetSegmentType(i);
                if (segmentType == Autodesk.AutoCAD.DatabaseServices.SegmentType.Arc)
                {
                    segment = new DTO.Segment();
                    segment.SegmentType = SegmentType.Arc;
                    var arc = polyline.GetArcSegmentAt(i);
                    segment.Data = JsonConvert.SerializeObject(arc.ToArcPayload());
                    polycurvePayload.Segments.Add(segment);
                }
                else if (segmentType == Autodesk.AutoCAD.DatabaseServices.SegmentType.Line)
                {
                    segment = new DTO.Segment();
                    segment.SegmentType = SegmentType.Line;
                    var line = polyline.GetLineSegmentAt(i);
                    segment.Data = JsonConvert.SerializeObject(line.ToLinePayload());
                    polycurvePayload.Segments.Add(segment);
                }
            }

            var properties = new Dictionary<string, dynamic>()
            {
                {"EndPoint", new DTO.PointPayload {Value = new List<double> {polyline.EndPoint.X, polyline.EndPoint.Y, polyline.EndPoint.Z}}},
                {"Layer", polyline.Layer},
                {"Length", polyline.Length},
                {"StartPoint", new DTO.PointPayload {Value = new List<double> {polyline.StartPoint.X, polyline.StartPoint.Y, polyline.StartPoint.Z}}},
            };

            polycurvePayload.Properties = properties;                   
            return polycurvePayload;
        }

        public static DTO.ArcPayload ToArcPayload(this CircularArc3d arc)
        {
            //Whilst AutoCAD has a number of angle display formats, the actual angle unit is the radian.
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
                    Value = new List<double> { arc.Center.X, arc.Center.Y, arc.Center.Z }
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

            var sweep = GetSweep(arc.StartAngle, arc.EndAngle);
            var arcPayload = new DTO.ArcPayload()
            {
                Plane = planePayload,
                Radius = arc.Radius,
                StartAngle = 0,
                EndAngle = sweep, //EndAngle is also the sweep because start angle is always 0
                AngleRadians = sweep              
            };

            return arcPayload;
        }

        private static double GetSweep(double startAngle, double endAngle)
        {
            var sweep = endAngle - startAngle;
            if (sweep < 0)
            {
                sweep += 2 * Math.PI;
            }

            return sweep;
        }

        private static Point3d ToWCS(Point3d pt, Vector3d normal)
        {
            var transform = Matrix3d.WorldToPlane(normal);
            return pt.TransformBy(transform);
        }
    }
}
