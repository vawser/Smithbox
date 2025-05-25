using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Scene.DebugPrimitives;

public class DbgPrimSolidBox : DbgPrimSolid
{
    private readonly DbgPrimGeometryData GeometryData;

    /// Set this so the box isn't culled incorrectly
    public override BoundingBox Bounds => new(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

    public DbgPrimSolidBox(Transform location, Vector3 min, Vector3 max, Color color)
    {
        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            NameColor = color;

            // 3 Letters of below names: 
            // [T]op/[B]ottom, [F]ront/[B]ack, [L]eft/[R]ight
            var tfl = new Vector3(min.X, max.Y, max.Z);
            var tfr = new Vector3(max.X, max.Y, max.Z);
            var bfr = new Vector3(max.X, min.Y, max.Z);
            var bfl = new Vector3(min.X, min.Y, max.Z);
            var tbl = new Vector3(min.X, max.Y, min.Z);
            var tbr = new Vector3(max.X, max.Y, min.Z);
            var bbr = new Vector3(max.X, min.Y, min.Z);
            var bbl = new Vector3(min.X, min.Y, min.Z);

            var topColor = color;
            var bottomColor = color;
            var frontColor = color;
            var backColor = color;
            var leftColor = color;
            var rightColor = color;

            // Top Face
            AddTri(tfl, tbl, tbr, topColor);
            AddTri(tfl, tbr, tfr, topColor);

            // Bottom Face
            AddTri(bbl, bfl, bbr, bottomColor);
            AddTri(bfl, bfr, bbr, bottomColor);

            // Front Face
            AddTri(bfl, tfl, tfr, frontColor);
            AddTri(bfl, tfr, bfr, frontColor);

            // Back Face 
            AddTri(bbr, tbr, tbl, backColor); 
            AddTri(bbr, tbl, bbl, backColor); 

            // Left Face
            AddTri(bbl, tbl, tfl, leftColor);
            AddTri(bbl, tfl, bfl, leftColor);

            // Right Face
            AddTri(bfr, tfr, tbr, rightColor);
            AddTri(bfr, tbr, bbr, rightColor);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }

        Renderer.AddBackgroundUploadTask((d, cl) =>
        {
            UpdatePerFrameResources(d, cl, null);
        });
    }
}
