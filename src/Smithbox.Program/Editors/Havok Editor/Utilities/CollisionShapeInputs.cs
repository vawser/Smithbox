using System.ComponentModel.DataAnnotations;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// Which way the faces of a created collision shape point
/// </summary>
public enum CollisionFacing
{
    [Display(Name = "HAVOK_CollisionGeneratorFacing_Up")]
    Up,
    [Display(Name = "HAVOK_CollisionGeneratorFacing_Down")]
    Down
}

/// <summary>
/// The inputs of the simple shapes <see cref="HavokShapeGenerator"/> builds
/// </summary>
public static class CollisionShapeInputs
{
    public class SquareParameters
    {
        public float Length { get; set; } = 1;
        public float Width { get; set; } = 1;
    }

    public class TriangleParameters
    {
        public float Size { get; set; } = 1;
    }

    public class CircleParameters
    {
        public float Radius { get; set; } = 1;
        public int Segments { get; set; } = 24;
    }

    public class SemiCircleParameters
    {
        public float Radius { get; set; } = 1;
        public int Segments { get; set; } = 24;
    }
}
