// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpShapeCastViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpShapeCastViewerShapeCastOptionsData : HavokData<ShapeCastOptions> 
{
    public hknpShapeCastViewerShapeCastOptionsData(HavokType type, ShapeCastOptions instance) : base(type, instance) {}

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
            case "m_drawCastCollector":
            case "drawCastCollector":
            {
                if (instance.m_drawCastCollector is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawStartCollector":
            case "drawStartCollector":
            {
                if (instance.m_drawStartCollector is not TGet castValue) return false;
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
            case "m_drawCastCollector":
            case "drawCastCollector":
            {
                if (value is not bool castValue) return false;
                instance.m_drawCastCollector = castValue;
                return true;
            }
            case "m_drawStartCollector":
            case "drawStartCollector":
            {
                if (value is not bool castValue) return false;
                instance.m_drawStartCollector = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
