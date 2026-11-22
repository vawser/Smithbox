// Automatically Generated

#nullable enable

namespace Havok.hk2012.Accessors;

internal sealed class hkFourTransposedPointsData : Havok.Formats.HavokFieldAccess<Havok.hk2012.hkFourTransposedPoints>
{
    private static readonly System.Reflection.FieldInfo _verticesInfo = typeof(Havok.hk2012.hkFourTransposedPoints).GetField("m_vertices")!;
    public hkFourTransposedPointsData(Havok.hk2012.hkFourTransposedPoints instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                if (value is not Vector4[] castValue || castValue.Length != 3) return false;
                try
                {
                    _verticesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
