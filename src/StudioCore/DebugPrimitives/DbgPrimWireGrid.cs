using System.Drawing;
using System.Numerics;

namespace StudioCore.DebugPrimitives;

public class DbgPrimWireGrid : DbgPrimWire
{
    public DbgPrimWireGrid(Color originColor, Color color, int unitRange, float unitSize)
    {
        NameColor = color;
        for (var h = -unitRange; h <= unitRange; h++)
        {
            AddLine(new Vector3(h, 0, unitRange) * unitSize,
                new Vector3(h, 0, -unitRange) * unitSize, h == 0 ? originColor : color);
        }

        for (var v = -unitRange; v <= unitRange; v++)
        {
            AddLine(new Vector3(-unitRange, 0, v) * unitSize,
                new Vector3(unitRange, 0, v) * unitSize, v == 0 ? originColor : color);
        }
    }
}
