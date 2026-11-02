// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpShapeViewerOptionsLevelOfDetailData : HavokData<HKLib.hk2019.hknpShapeViewer.Options.LevelOfDetail> 
{
    public hknpShapeViewerOptionsLevelOfDetailData(HavokType type, HKLib.hk2019.hknpShapeViewer.Options.LevelOfDetail instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_high":
            case "high":
            {
                if (instance.m_high is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_medium":
            case "medium":
            {
                if (instance.m_medium is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_low":
            case "low":
            {
                if (instance.m_low is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_user0":
            case "user0":
            {
                if (instance.m_user0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_user1":
            case "user1":
            {
                if (instance.m_user1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_user2":
            case "user2":
            {
                if (instance.m_user2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_user3":
            case "user3":
            {
                if (instance.m_user3 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_user4":
            case "user4":
            {
                if (instance.m_user4 is not TGet castValue) return false;
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
