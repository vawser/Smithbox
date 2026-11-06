// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpShapeViewerOptionsLevelOfDetailData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpShapeViewer.Options.LevelOfDetail>
{
    public hknpShapeViewerOptionsLevelOfDetailData(Havok.hk2018.hknpShapeViewer.Options.LevelOfDetail instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maximum":
            case "maximum":
            {
                value = instance.m_maximum;
                return true;
            }
            case "m_high":
            case "high":
            {
                value = instance.m_high;
                return true;
            }
            case "m_default":
            case "default":
            {
                value = instance.m_default;
                return true;
            }
            case "m_simplified":
            case "simplified":
            {
                value = instance.m_simplified;
                return true;
            }
            case "m_approximate":
            case "approximate":
            {
                value = instance.m_approximate;
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
            case "m_maximum":
            case "maximum":
            {
                if (value is not bool castValue) return false;
                instance.m_maximum = castValue;
                return true;
            }
            case "m_high":
            case "high":
            {
                if (value is not bool castValue) return false;
                instance.m_high = castValue;
                return true;
            }
            case "m_default":
            case "default":
            {
                if (value is not bool castValue) return false;
                instance.m_default = castValue;
                return true;
            }
            case "m_simplified":
            case "simplified":
            {
                if (value is not bool castValue) return false;
                instance.m_simplified = castValue;
                return true;
            }
            case "m_approximate":
            case "approximate":
            {
                if (value is not bool castValue) return false;
                instance.m_approximate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
