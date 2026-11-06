// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclClothContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclClothContainer>
{
    public hclClothContainerData(Havok.hk2018.hclClothContainer instance) : base(instance) { }

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
            case "m_collidables":
            case "collidables":
            {
                value = instance.m_collidables;
                return true;
            }
            case "m_clothDatas":
            case "clothDatas":
            {
                value = instance.m_clothDatas;
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
            case "m_collidables":
            case "collidables":
            {
                if (value is not List<Havok.hk2018.hclCollidable?> castValue) return false;
                instance.m_collidables = castValue;
                return true;
            }
            case "m_clothDatas":
            case "clothDatas":
            {
                if (value is not List<Havok.hk2018.hclClothData?> castValue) return false;
                instance.m_clothDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
