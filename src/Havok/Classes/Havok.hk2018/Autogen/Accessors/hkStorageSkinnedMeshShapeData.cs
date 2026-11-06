// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkStorageSkinnedMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkStorageSkinnedMeshShape>
{
    public hkStorageSkinnedMeshShapeData(Havok.hk2018.hkStorageSkinnedMeshShape instance) : base(instance) { }

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
            case "m_bonesBuffer":
            case "bonesBuffer":
            {
                value = instance.m_bonesBuffer;
                return true;
            }
            case "m_boneSets":
            case "boneSets":
            {
                value = instance.m_boneSets;
                return true;
            }
            case "m_boneSections":
            case "boneSections":
            {
                value = instance.m_boneSections;
                return true;
            }
            case "m_parts":
            case "parts":
            {
                value = instance.m_parts;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_bonesBuffer":
            case "bonesBuffer":
            {
                if (value is not List<short> castValue) return false;
                instance.m_bonesBuffer = castValue;
                return true;
            }
            case "m_boneSets":
            case "boneSets":
            {
                if (value is not List<Havok.hk2018.hkSkinnedMeshShape.BoneSet> castValue) return false;
                instance.m_boneSets = castValue;
                return true;
            }
            case "m_boneSections":
            case "boneSections":
            {
                if (value is not List<Havok.hk2018.hkSkinnedMeshShape.BoneSection> castValue) return false;
                instance.m_boneSections = castValue;
                return true;
            }
            case "m_parts":
            case "parts":
            {
                if (value is not List<Havok.hk2018.hkSkinnedMeshShape.Part> castValue) return false;
                instance.m_parts = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
