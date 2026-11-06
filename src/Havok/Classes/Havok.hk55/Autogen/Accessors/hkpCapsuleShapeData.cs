// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpCapsuleShapeData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpCapsuleShape>
{
    public hkpCapsuleShapeData(Havok.hk55.hkpCapsuleShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_vertexA":
            case "vertexA":
            {
                value = instance.m_vertexA;
                return true;
            }
            case "m_vertexB":
            case "vertexB":
            {
                value = instance.m_vertexB;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_vertexA":
            case "vertexA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vertexA = castValue;
                return true;
            }
            case "m_vertexB":
            case "vertexB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vertexB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
