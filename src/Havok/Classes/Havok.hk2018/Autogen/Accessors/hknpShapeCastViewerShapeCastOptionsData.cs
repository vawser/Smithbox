// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpShapeCastViewerShapeCastOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpShapeCastViewer.ShapeCastOptions>
{
    public hknpShapeCastViewerShapeCastOptionsData(Havok.hk2018.hknpShapeCastViewer.ShapeCastOptions instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorBasedOnTime":
            case "colorBasedOnTime":
            {
                value = instance.m_colorBasedOnTime;
                return true;
            }
            case "m_maxTimeMs":
            case "maxTimeMs":
            {
                value = instance.m_maxTimeMs;
                return true;
            }
            case "m_drawCastCollector":
            case "drawCastCollector":
            {
                value = instance.m_drawCastCollector;
                return true;
            }
            case "m_drawStartCollector":
            case "drawStartCollector":
            {
                value = instance.m_drawStartCollector;
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
