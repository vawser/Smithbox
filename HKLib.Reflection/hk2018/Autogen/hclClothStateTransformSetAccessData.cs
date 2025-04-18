// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclClothStateTransformSetAccessData : HavokData<hclClothState.TransformSetAccess> 
{
    public hclClothStateTransformSetAccessData(HavokType type, hclClothState.TransformSetAccess instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transformSetIndex":
            case "transformSetIndex":
            {
                if (instance.m_transformSetIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformSetUsage":
            case "transformSetUsage":
            {
                if (instance.m_transformSetUsage is not TGet castValue) return false;
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
                if (value is not hclTransformSetUsage castValue) return false;
                instance.m_transformSetUsage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
