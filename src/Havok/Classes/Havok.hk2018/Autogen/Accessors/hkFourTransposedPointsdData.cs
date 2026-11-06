// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkFourTransposedPointsdData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkFourTransposedPointsd>
{
    private static readonly System.Reflection.FieldInfo _verticesInfo = typeof(Havok.hk2018.hkFourTransposedPointsd).GetField("m_vertices")!;
    public hkFourTransposedPointsdData(Havok.hk2018.hkFourTransposedPointsd instance) : base(instance) { }

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
