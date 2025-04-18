// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothSetupContainerData : HavokData<hclClothSetupContainer> 
{
    public hclClothSetupContainerData(HavokType type, hclClothSetupContainer instance) : base(type, instance) {}

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
            case "m_clothSetupDatas":
            case "clothSetupDatas":
            {
                if (instance.m_clothSetupDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_namedSetupMeshWrappers":
            case "namedSetupMeshWrappers":
            {
                if (instance.m_namedSetupMeshWrappers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_namedTransformSetWrappers":
            case "namedTransformSetWrappers":
            {
                if (instance.m_namedTransformSetWrappers is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_clothSetupDatas":
            case "clothSetupDatas":
            {
                if (value is not List<hclClothSetupObject?> castValue) return false;
                instance.m_clothSetupDatas = castValue;
                return true;
            }
            case "m_namedSetupMeshWrappers":
            case "namedSetupMeshWrappers":
            {
                if (value is not List<hclNamedSetupMesh?> castValue) return false;
                instance.m_namedSetupMeshWrappers = castValue;
                return true;
            }
            case "m_namedTransformSetWrappers":
            case "namedTransformSetWrappers":
            {
                if (value is not List<hclNamedTransformSetSetupObject?> castValue) return false;
                instance.m_namedTransformSetWrappers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
