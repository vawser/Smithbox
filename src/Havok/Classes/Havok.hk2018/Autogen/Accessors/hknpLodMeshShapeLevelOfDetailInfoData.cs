// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpLodMeshShapeLevelOfDetailInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpLodMeshShape.LevelOfDetailInfo>
{
    public hknpLodMeshShapeLevelOfDetailInfoData(Havok.hk2018.hknpLodMeshShape.LevelOfDetailInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_levelOfDetail":
            case "levelOfDetail":
            {
                value = instance.m_levelOfDetail;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                value = instance.m_maxDistance;
                return true;
            }
            case "m_maxShrink":
            case "maxShrink":
            {
                value = instance.m_maxShrink;
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
