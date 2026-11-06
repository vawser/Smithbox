// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpCylinderShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpCylinderShape>
{
    public hkpCylinderShapeData(Havok.hk2010.hkpCylinderShape instance) : base(instance) { }

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
            case "m_cylRadius":
            case "cylRadius":
            {
                value = instance.m_cylRadius;
                return true;
            }
            case "m_cylBaseRadiusFactorForHeightFieldCollisions":
            case "cylBaseRadiusFactorForHeightFieldCollisions":
            {
                value = instance.m_cylBaseRadiusFactorForHeightFieldCollisions;
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
            case "m_perpendicular1":
            case "perpendicular1":
            {
                value = instance.m_perpendicular1;
                return true;
            }
            case "m_perpendicular2":
            case "perpendicular2":
            {
                value = instance.m_perpendicular2;
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
            case "m_cylRadius":
            case "cylRadius":
            {
                if (value is not float castValue) return false;
                instance.m_cylRadius = castValue;
                return true;
            }
            case "m_cylBaseRadiusFactorForHeightFieldCollisions":
            case "cylBaseRadiusFactorForHeightFieldCollisions":
            {
                if (value is not float castValue) return false;
                instance.m_cylBaseRadiusFactorForHeightFieldCollisions = castValue;
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
            case "m_perpendicular1":
            case "perpendicular1":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_perpendicular1 = castValue;
                return true;
            }
            case "m_perpendicular2":
            case "perpendicular2":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_perpendicular2 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
