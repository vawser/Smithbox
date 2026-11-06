// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclClothStateTransformSetAccessData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclClothState.TransformSetAccess>
{
    public hclClothStateTransformSetAccessData(Havok.hk2016.hclClothState.TransformSetAccess instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                value = instance.m_transformSetIndex;
                return true;
            }
            case "m_transformSetUsage":
            case "transformSetUsage":
            {
                value = instance.m_transformSetUsage;
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
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_transformSetIndex = castValue;
                return true;
            }
            case "m_transformSetUsage":
            case "transformSetUsage":
            {
                if (value is not Havok.hk2016.hclTransformSetUsage castValue) return false;
                instance.m_transformSetUsage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
