// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclCapsuleShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclCapsuleShape>
{
    public hclCapsuleShapeData(Havok.hk2018.hclCapsuleShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_start":
            case "start":
            {
                value = instance.m_start;
                return true;
            }
            case "m_end":
            case "end":
            {
                value = instance.m_end;
                return true;
            }
            case "m_dir":
            case "dir":
            {
                value = instance.m_dir;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_capLenSqrdInv":
            case "capLenSqrdInv":
            {
                value = instance.m_capLenSqrdInv;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_start":
            case "start":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            case "m_dir":
            case "dir":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_dir = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_capLenSqrdInv":
            case "capLenSqrdInv":
            {
                if (value is not float castValue) return false;
                instance.m_capLenSqrdInv = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
