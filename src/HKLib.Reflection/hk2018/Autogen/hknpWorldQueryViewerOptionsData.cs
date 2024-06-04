// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpWorldQueryViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpWorldQueryViewerOptionsData : HavokData<Options> 
{
    public hknpWorldQueryViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorBasedOnTime":
            case "colorBasedOnTime":
            {
                if (instance.m_colorBasedOnTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTimeMs":
            case "maxTimeMs":
            {
                if (instance.m_maxTimeMs is not TGet castValue) return false;
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
            case "m_colorBasedOnTime":
            case "colorBasedOnTime":
            {
                if (value is not bool castValue) return false;
                instance.m_colorBasedOnTime = castValue;
                return true;
            }
            case "m_maxTimeMs":
            case "maxTimeMs":
            {
                if (value is not float castValue) return false;
                instance.m_maxTimeMs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
