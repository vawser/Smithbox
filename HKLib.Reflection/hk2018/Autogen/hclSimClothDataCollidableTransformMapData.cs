// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimClothDataCollidableTransformMapData : HavokData<hclSimClothData.CollidableTransformMap> 
{
    public hclSimClothDataCollidableTransformMapData(HavokType type, hclSimClothData.CollidableTransformMap instance) : base(type, instance) {}

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
            case "m_transformIndices":
            case "transformIndices":
            {
                if (instance.m_transformIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offsets":
            case "offsets":
            {
                if (instance.m_offsets is not TGet castValue) return false;
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
                if (value is not int castValue) return false;
                instance.m_transformSetIndex = castValue;
                return true;
            }
            case "m_transformIndices":
            case "transformIndices":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_transformIndices = castValue;
                return true;
            }
            case "m_offsets":
            case "offsets":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_offsets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
