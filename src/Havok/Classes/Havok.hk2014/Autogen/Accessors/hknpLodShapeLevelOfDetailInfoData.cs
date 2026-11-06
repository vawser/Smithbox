// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpLodShapeLevelOfDetailInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpLodShapeLevelOfDetailInfo>
{
    public hknpLodShapeLevelOfDetailInfoData(Havok.hk2014.hknpLodShapeLevelOfDetailInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
