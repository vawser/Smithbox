// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkStorageSkinnedMeshShapeData : HavokData<hkStorageSkinnedMeshShape> 
{
    public hkStorageSkinnedMeshShapeData(HavokType type, hkStorageSkinnedMeshShape instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bonesBuffer":
            case "bonesBuffer":
            {
                if (instance.m_bonesBuffer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneSets":
            case "boneSets":
            {
                if (instance.m_boneSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneSections":
            case "boneSections":
            {
                if (instance.m_boneSections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parts":
            case "parts":
            {
                if (instance.m_parts is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is not List<hkSkinnedMeshShape.BoneSet> castValue) return false;
                instance.m_boneSets = castValue;
                return true;
            }
            case "m_boneSections":
            case "boneSections":
            {
                if (value is not List<hkSkinnedMeshShape.BoneSection> castValue) return false;
                instance.m_boneSections = castValue;
                return true;
            }
            case "m_parts":
            case "parts":
            {
                if (value is not List<hkSkinnedMeshShape.Part> castValue) return false;
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
