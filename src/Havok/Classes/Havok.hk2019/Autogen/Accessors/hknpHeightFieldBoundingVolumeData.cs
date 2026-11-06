// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpHeightFieldBoundingVolumeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpHeightFieldBoundingVolume>
{
    public hknpHeightFieldBoundingVolumeData(Havok.hk2019.hknpHeightFieldBoundingVolume instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minMaxTree":
            case "minMaxTree":
            {
                value = instance.m_minMaxTree;
                return true;
            }
            case "m_minLevel":
            case "minLevel":
            {
                value = instance.m_minLevel;
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
            case "m_minMaxTree":
            case "minMaxTree":
            {
                if (value is not Havok.hk2019.hknpMinMaxQuadTree castValue) return false;
                instance.m_minMaxTree = castValue;
                return true;
            }
            case "m_minLevel":
            case "minLevel":
            {
                if (value is not int castValue) return false;
                instance.m_minLevel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
