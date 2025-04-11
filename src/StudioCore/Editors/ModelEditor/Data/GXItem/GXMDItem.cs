using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Credit to RunDevelopment for this code
/// </summary>
public class GXMDItem
{
    public GXMDItemType Type { get; set; }
    public object Value { get; set; }
    public bool? Flagged { get; set; } = null;

    public GXMDItem(GXMDItemType type, object value)
    {
        Type = type;
        Value = value;
    }
    public GXMDItem() : this(GXMDItemType.Float, 0f) { }
    public GXMDItem(float value) : this(GXMDItemType.Float, value) { }
    public GXMDItem(Vector2 value) : this(GXMDItemType.Float2, value) { }
    public GXMDItem(Vector3 value) : this(GXMDItemType.Float3, value) { }
    public GXMDItem(Float5 value) : this(GXMDItemType.Float5, value) { }

    public void Validate()
    {
        void AssertType<T>()
        {
            if (Value is null)
                throw new FormatException($"Expected {Type} item to have a {typeof(T)} value but found null.");
            if (!(Value is T))
                throw new FormatException($"Expected {Type} item to have a {typeof(T)} value but found {Value.GetType()}.");
        }

        switch (Type)
        {
            case GXMDItemType.Float:
                AssertType<float>();
                break;
            case GXMDItemType.Float2:
                AssertType<Vector2>();
                break;
            case GXMDItemType.Float3:
                AssertType<Vector3>();
                break;
            case GXMDItemType.Float5:
                AssertType<Float5>();
                break;
            default:
                throw new FormatException($"{Type} is not a valid type.");
        }
    }
}