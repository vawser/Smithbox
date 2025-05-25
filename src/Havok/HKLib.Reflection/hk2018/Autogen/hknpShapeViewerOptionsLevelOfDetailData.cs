// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpShapeViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpShapeViewerOptionsLevelOfDetailData : HavokData<Options.LevelOfDetail> 
{
    public hknpShapeViewerOptionsLevelOfDetailData(HavokType type, Options.LevelOfDetail instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maximum":
            case "maximum":
            {
                if (instance.m_maximum is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_high":
            case "high":
            {
                if (instance.m_high is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_default":
            case "default":
            {
                if (instance.m_default is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simplified":
            case "simplified":
            {
                if (instance.m_simplified is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_approximate":
            case "approximate":
            {
                if (instance.m_approximate is not TGet castValue) return false;
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
