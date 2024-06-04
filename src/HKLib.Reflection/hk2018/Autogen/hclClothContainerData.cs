// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothContainerData : HavokData<hclClothContainer> 
{
    public hclClothContainerData(HavokType type, hclClothContainer instance) : base(type, instance) {}

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
            case "m_collidables":
            case "collidables":
            {
                if (instance.m_collidables is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clothDatas":
            case "clothDatas":
            {
                if (instance.m_clothDatas is not TGet castValue) return false;
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
            case "m_collidables":
            case "collidables":
            {
                if (value is not List<hclCollidable?> castValue) return false;
                instance.m_collidables = castValue;
                return true;
            }
            case "m_clothDatas":
            case "clothDatas":
            {
                if (value is not List<hclClothData?> castValue) return false;
                instance.m_clothDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
