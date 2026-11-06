// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkFourTransposedPointsImplDoubleData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkFourTransposedPointsImplDouble>
{
    private static readonly System.Reflection.FieldInfo _verticesInfo = typeof(Havok.hk2019.hkFourTransposedPointsImplDouble).GetField("m_vertices")!;
    public hkFourTransposedPointsImplDoubleData(Havok.hk2019.hkFourTransposedPointsImplDouble instance) : base(instance) { }

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
