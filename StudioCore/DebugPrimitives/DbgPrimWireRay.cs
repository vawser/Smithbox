using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireRay : DbgPrimWire
{
    private readonly DbgPrimGeometryData GeometryData;

    public DbgPrimWireRay(Transform location, Vector3 start, Vector3 end, Color color)
    {
        NameColor = color;


        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            // 3 Letters of below names: 
            // [T]op/[B]ottom, [F]ront/[B]ack, [L]eft/[R]ight

            // Top Face
            AddLine(start, end, color);

            //FinalizeBuffers(true);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }
    }

    public void UpdateTransform(Transform newTransform)
    {
    }
}
