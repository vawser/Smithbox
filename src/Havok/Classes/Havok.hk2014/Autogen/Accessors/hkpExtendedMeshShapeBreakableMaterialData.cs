// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpExtendedMeshShapeBreakableMaterialData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpExtendedMeshShapeBreakableMaterial>
{
    public hkpExtendedMeshShapeBreakableMaterialData(Havok.hk2014.hkpExtendedMeshShapeBreakableMaterial instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_strength":
            case "strength":
            {
                value = instance.m_strength;
                return true;
            }
            case "m_typeAndFlags":
            case "typeAndFlags":
            {
                value = instance.m_typeAndFlags;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
                return true;
            }
            case "m_subMaterials":
            case "subMaterials":
            {
                value = instance.m_subMaterials;
                return true;
            }
            case "m_inverseMapping":
            case "inverseMapping":
            {
                value = instance.m_inverseMapping;
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
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            case "m_typeAndFlags":
            case "typeAndFlags":
            {
                if (value is not int castValue) return false;
                instance.m_typeAndFlags = castValue;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                if (value is null)
                {
                    instance.m_properties = default;
                    return true;
                }
                if (value is Havok.hk2014.hkRefCountedProperties castValue)
                {
                    instance.m_properties = castValue;
                    return true;
                }
                return false;
            }
            case "m_subMaterials":
            case "subMaterials":
            {
                if (value is not List<Havok.hk2014.hkpBreakableMaterial?> castValue) return false;
                instance.m_subMaterials = castValue;
                return true;
            }
            case "m_inverseMapping":
            case "inverseMapping":
            {
                if (value is null)
                {
                    instance.m_inverseMapping = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpBreakableMultiMaterialInverseMapping castValue)
                {
                    instance.m_inverseMapping = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
