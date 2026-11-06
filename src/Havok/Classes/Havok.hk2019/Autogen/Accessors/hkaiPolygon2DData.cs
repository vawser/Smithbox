// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiPolygon2DData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiPolygon2D>
{
    public hkaiPolygon2DData(Havok.hk2019.hkaiPolygon2D instance) : base(instance) { }

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
                if (value is not List<Havok.hk2019.hkVector2> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
