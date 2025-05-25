#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;


/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public class GX00ItemValueDescriptor
{
    public string Name { get; set; } = "Unknown";
    public GX00ItemValueType Type { get; set; } = GX00ItemValueType.Unknown;
    // If the type is Int or Float, then this is the smallest accepted value.
    public float? Min { get; set; }
    // If the type is Int or Float, then this is the largest accepted value.
    public float? Max { get; set; }
    // If the type is Enum, then this lists all variants. This is a mapping from value to label.
    public Dictionary<int, string>? Enum { get; set; }

    public bool Fits(int value)
    {
        return Type switch
        {
            GX00ItemValueType.Unknown => value == 0,
            GX00ItemValueType.Int => (Min == null || Min.Value <= value) && (Max == null || Max.Value >= value),
            GX00ItemValueType.Enum => Enum != null && Enum.ContainsKey(value),
            GX00ItemValueType.Bool => value == 0 || value == 1,
            _ => false
        };
    }
    public bool Fits(float value)
    {
        return Type switch
        {
            GX00ItemValueType.Unknown => value == 0f,
            GX00ItemValueType.Float => (Min == null || Min.Value <= value) && (Max == null || Max.Value >= value),
            _ => false
        };
    }
    public bool Fits(GXValue value)
    {
        return Type switch
        {
            GX00ItemValueType.Float => Fits(value.F),
            _ => Fits(value.I)
        };
    }
}