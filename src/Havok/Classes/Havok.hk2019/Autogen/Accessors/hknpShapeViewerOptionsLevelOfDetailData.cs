// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpShapeViewerOptionsLevelOfDetailData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpShapeViewer.Options.LevelOfDetail>
{
    public hknpShapeViewerOptionsLevelOfDetailData(Havok.hk2019.hknpShapeViewer.Options.LevelOfDetail instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_high":
            case "high":
            {
                value = instance.m_high;
                return true;
            }
            case "m_medium":
            case "medium":
            {
                value = instance.m_medium;
                return true;
            }
            case "m_low":
            case "low":
            {
                value = instance.m_low;
                return true;
            }
            case "m_user0":
            case "user0":
            {
                value = instance.m_user0;
                return true;
            }
            case "m_user1":
            case "user1":
            {
                value = instance.m_user1;
                return true;
            }
            case "m_user2":
            case "user2":
            {
                value = instance.m_user2;
                return true;
            }
            case "m_user3":
            case "user3":
            {
                value = instance.m_user3;
                return true;
            }
            case "m_user4":
            case "user4":
            {
                value = instance.m_user4;
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
            case "m_high":
            case "high":
            {
                if (value is not bool castValue) return false;
                instance.m_high = castValue;
                return true;
            }
            case "m_medium":
            case "medium":
            {
                if (value is not bool castValue) return false;
                instance.m_medium = castValue;
                return true;
            }
            case "m_low":
            case "low":
            {
                if (value is not bool castValue) return false;
                instance.m_low = castValue;
                return true;
            }
            case "m_user0":
            case "user0":
            {
                if (value is not bool castValue) return false;
                instance.m_user0 = castValue;
                return true;
            }
            case "m_user1":
            case "user1":
            {
                if (value is not bool castValue) return false;
                instance.m_user1 = castValue;
                return true;
            }
            case "m_user2":
            case "user2":
            {
                if (value is not bool castValue) return false;
                instance.m_user2 = castValue;
                return true;
            }
            case "m_user3":
            case "user3":
            {
                if (value is not bool castValue) return false;
                instance.m_user3 = castValue;
                return true;
            }
            case "m_user4":
            case "user4":
            {
                if (value is not bool castValue) return false;
                instance.m_user4 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
