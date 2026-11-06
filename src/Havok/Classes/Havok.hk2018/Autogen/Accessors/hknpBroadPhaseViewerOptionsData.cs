// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpBroadPhaseViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpBroadPhaseViewer.Options>
{
    public hknpBroadPhaseViewerOptionsData(Havok.hk2018.hknpBroadPhaseViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorByLayer":
            case "colorByLayer":
            {
                value = instance.m_colorByLayer;
                return true;
            }
            case "m_showPreviousAabbs":
            case "showPreviousAabbs":
            {
                value = instance.m_showPreviousAabbs;
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
            case "m_colorByLayer":
            case "colorByLayer":
            {
                if (value is not bool castValue) return false;
                instance.m_colorByLayer = castValue;
                return true;
            }
            case "m_showPreviousAabbs":
            case "showPreviousAabbs":
            {
                if (value is not bool castValue) return false;
                instance.m_showPreviousAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
