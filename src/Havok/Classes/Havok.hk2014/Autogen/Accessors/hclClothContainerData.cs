// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclClothContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclClothContainer>
{
    public hclClothContainerData(Havok.hk2014.hclClothContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_collidables":
            case "collidables":
            {
                if (value is not List<Havok.hk2014.hclCollidable?> castValue) return false;
                instance.m_collidables = castValue;
                return true;
            }
            case "m_clothDatas":
            case "clothDatas":
            {
                if (value is not List<Havok.hk2014.hclClothData?> castValue) return false;
                instance.m_clothDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
