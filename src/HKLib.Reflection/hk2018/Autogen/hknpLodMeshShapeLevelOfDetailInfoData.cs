// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpLodMeshShapeLevelOfDetailInfoData : HavokData<hknpLodMeshShape.LevelOfDetailInfo> 
{
    public hknpLodMeshShapeLevelOfDetailInfoData(HavokType type, hknpLodMeshShape.LevelOfDetailInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                if (instance.m_levelOfDetail is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (instance.m_maxDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxShrink":
            case "maxShrink":
            {
                if (instance.m_maxShrink is not TGet castValue) return false;
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
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                if (value is not byte castValue) return false;
                instance.m_levelOfDetail = castValue;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistance = castValue;
                return true;
            }
            case "m_maxShrink":
            case "maxShrink":
            {
                if (value is not float castValue) return false;
                instance.m_maxShrink = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
