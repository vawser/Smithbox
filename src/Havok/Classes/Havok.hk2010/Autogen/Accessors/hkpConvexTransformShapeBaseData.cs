// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpConvexTransformShapeBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpConvexTransformShapeBase>
{
    public hkpConvexTransformShapeBaseData(Havok.hk2010.hkpConvexTransformShapeBase instance) : base(instance) { }

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
            case "m_childShape":
            case "childShape":
            {
                value = instance.m_childShape;
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
            case "m_childShape":
            case "childShape":
            {
                if (value is not Havok.hk2010.hkpSingleShapeContainer castValue) return false;
                instance.m_childShape = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
