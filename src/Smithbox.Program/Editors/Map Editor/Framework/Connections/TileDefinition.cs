using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class TileDefinition
{
    public int TileX { get; set; }
    public int TileZ { get; set; }
    public Vector3 TileOffset { get; set; }

    public TileDefinition() { }
    public TileDefinition(int tileX, int tileZ, Vector3 offset)
    {
        TileX = tileX;
        TileZ = tileZ;
        TileOffset = offset;
    }
}
