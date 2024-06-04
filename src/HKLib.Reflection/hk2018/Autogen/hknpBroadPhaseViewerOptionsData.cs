// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpBroadPhaseViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpBroadPhaseViewerOptionsData : HavokData<Options> 
{
    public hknpBroadPhaseViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_colorByLayer":
            case "colorByLayer":
            {
                if (instance.m_colorByLayer is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showPreviousAabbs":
            case "showPreviousAabbs":
            {
                if (instance.m_showPreviousAabbs is not TGet castValue) return false;
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
