using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireBone : DbgPrimWire
{
    public const float ThicknessRatio = 0.25f;

    private static DbgPrimGeometryData GeometryData;

    public DbgPrimWireBone(string name, Transform location, Color color)
    {
        NameColor = color;
        Name = name;

        if (GeometryData != null)
        {
            SetBuffers(GeometryData.GeomBuffer);
        }
        else
        {
            Vector3 pt_start = Vector3.Zero;
            Vector3 pt_cardinal1 = (Vector3.UnitY * ThicknessRatio) + (Vector3.UnitX * ThicknessRatio);
            Vector3 pt_cardinal2 = (Vector3.UnitZ * ThicknessRatio) + (Vector3.UnitX * ThicknessRatio);
            Vector3 pt_cardinal3 = (-Vector3.UnitY * ThicknessRatio) + (Vector3.UnitX * ThicknessRatio);
            Vector3 pt_cardinal4 = (-Vector3.UnitZ * ThicknessRatio) + (Vector3.UnitX * ThicknessRatio);
            Vector3 pt_tip = Vector3.UnitX;

            //Start to cardinals
            AddLine(pt_start, pt_cardinal1);
            AddLine(pt_start, pt_cardinal2);
            AddLine(pt_start, pt_cardinal3);
            AddLine(pt_start, pt_cardinal4);

            //Cardinals to end
            AddLine(pt_cardinal1, pt_tip);
            AddLine(pt_cardinal2, pt_tip);
            AddLine(pt_cardinal3, pt_tip);
            AddLine(pt_cardinal4, pt_tip);

            //Connecting the cardinals
            AddLine(pt_cardinal1, pt_cardinal2);
            AddLine(pt_cardinal2, pt_cardinal3);
            AddLine(pt_cardinal3, pt_cardinal4);
            AddLine(pt_cardinal4, pt_cardinal1);

            //FinalizeBuffers(true);

            GeometryData = new DbgPrimGeometryData { GeomBuffer = GeometryBuffer };
        }
    }
}
