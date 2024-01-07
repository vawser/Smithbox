using System;
using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireCapsule : DbgPrimWire
{
    public readonly DbgPrimWireCapsule_End HemisphereA;
    public readonly DbgPrimWireCapsule_End HemisphereB;
    public readonly DbgPrimWireCapsule_Middle Midst;

    public DbgPrimWireCapsule(Color color)
    {
        HemisphereA = new DbgPrimWireCapsule_End { Category = DbgPrimCategory.DummyPolyHelper };
        Midst = new DbgPrimWireCapsule_Middle { Category = DbgPrimCategory.DummyPolyHelper };
        HemisphereB = new DbgPrimWireCapsule_End { Category = DbgPrimCategory.DummyPolyHelper };

        //Children.Add(HemisphereA);
        //Children.Add(Midst);
        //Children.Add(HemisphereB);
    }

    public void UpdateCapsuleEndPoints(Vector3 a, Vector3 b, float radius)
    {
        var dist = (b - a).Length();

        Matrix4x4 mtHemisphereA = Matrix4x4.Identity;
        Matrix4x4 mtMidst = Matrix4x4.Identity;
        Matrix4x4 mtHemisphereB = Matrix4x4.Identity;

        mtHemisphereA *= Matrix4x4.CreateScale(Vector3.One * radius);
        mtHemisphereA *= Matrix4x4.CreateRotationX(-Utils.PiOver2);

        mtMidst *= Matrix4x4.CreateScale(new Vector3(radius, dist, radius));
        mtMidst *= Matrix4x4.CreateRotationX(Utils.PiOver2);

        mtHemisphereB *= Matrix4x4.CreateScale(Vector3.One * radius);
        mtHemisphereB *= Matrix4x4.CreateRotationX(Utils.PiOver2);
        mtHemisphereB *= Matrix4x4.CreateTranslation(new Vector3(0, 0, dist));

        //HemisphereA.Transform = new Transform(mtHemisphereA);
        //Midst.Transform = new Transform(mtMidst);
        //HemisphereB.Transform = new Transform(mtHemisphereB);

        Vector3 forward = -Vector3.Normalize(b - a);

        var hitboxMatrix = Matrix4x4.CreateWorld(a, forward, Vector3.UnitY);

        if (forward.X == 0 && forward.Z == 0)
        {
            if (forward.Y >= 0)
            {
                hitboxMatrix = Matrix4x4.CreateRotationX(Utils.PiOver2) * Matrix4x4.CreateTranslation(a);
            }
            else
            {
                hitboxMatrix = Matrix4x4.CreateRotationX(-Utils.PiOver2) * Matrix4x4.CreateTranslation(a);
            }
        }

        //Matrix hitboxMatrix = Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.Normalize(b - a), 0)) * Matrix.CreateTranslation(a);

        //Transform = new Transform(hitboxMatrix);
    }

    public class DbgPrimWireCapsule_End : DbgPrimWire
    {
        public const int Segments = 12;
        private static DbgPrimGeometryData GeometryData;

        public DbgPrimWireCapsule_End()
        {
            //if (!(Segments >= 4))
            //    throw new ArgumentException($"Number of segments must be >= 4", nameof(Segments));

            if (GeometryData != null)
            {
                SetBuffers(GeometryData.GeomBuffer);
            }
            else
            {
                Vector3 topPoint = Vector3.UnitY * 1;
                Vector3 bottomPoint = -Vector3.UnitY * 1;
                var points = new Vector3[Segments, Segments];

                var verticalSegments = Segments / 2;

                for (var i = 0; i <= verticalSegments; i++)
                {
                    for (var j = 0; j < Segments; j++)
                    {
                        var horizontalAngle = 1.0f * j / Segments * Utils.Pi * 2.0f;
                        var verticalAngle = 1.0f * i / verticalSegments * Utils.PiOver2;
                        var altitude = (float)Math.Sin(verticalAngle);
                        var horizontalDist = (float)Math.Cos(verticalAngle);
                        points[i, j] = new Vector3((float)Math.Cos(horizontalAngle) * horizontalDist, altitude,
                            (float)Math.Sin(horizontalAngle) * horizontalDist) * 1;
                    }
                }

                for (var i = 0; i <= verticalSegments; i++)
                {
                    for (var j = 0; j < Segments; j++)
                    {
                        //// On the bottom, we must connect each to the bottom point
                        //if (i == 0)
                        //{
                        //    AddLine(points[i, j], bottomPoint, Color.White);
                        //}

                        // On the top, we must connect each point to the top
                        // Note: this isn't "else if" because with 2 segments, 
                        // these are both true for the only ring
                        if (i == Segments - 1)
                        {
                            AddLine(points[i, j], topPoint, Color.White);
                        }

                        // Make vertical lines that connect from this 
                        // horizontal ring to the one above
                        // Since we are connecting 
                        // (current) -> (the one above current)
                        // we dont need to do this for the very last one.
                        if (i < Segments - 1)
                        {
                            AddLine(points[i, j], points[i + 1, j], Color.White);
                        }


                        // Make lines that connect points horizontally
                        //---- if we reach end, we must wrap around, 
                        //---- otherwise, simply make line to next one
                        if (j == Segments - 1)
                        {
                            AddLine(points[i, j], points[i, 0], Color.White);
                        }
                        else
                        {
                            AddLine(points[i, j], points[i, j + 1], Color.White);
                        }
                    }
                }

                //FinalizeBuffers(true);

                GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
            }
        }
    }

    public class DbgPrimWireCapsule_Middle : DbgPrimWire
    {
        public const int Segments = 12;
        private static DbgPrimGeometryData GeometryData;

        public DbgPrimWireCapsule_Middle()
        {
            if (GeometryData != null)
            {
                SetBuffers(GeometryData.GeomBuffer);
            }
            else
            {
                for (var i = 0; i < Segments; i++)
                {
                    var horizontalAngle = 1.0f * i / Segments * Utils.Pi * 2.0f;
                    Vector3 a = new((float)Math.Cos(horizontalAngle), 0, (float)Math.Sin(horizontalAngle));
                    Vector3 b = new(a.X, 1, a.Z);
                    AddLine(a, b, Color.White);
                }

                //FinalizeBuffers(true);

                GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
            }
        }
    }
}
