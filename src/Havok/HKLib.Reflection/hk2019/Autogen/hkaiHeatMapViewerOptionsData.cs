// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiHeatMapViewerOptionsData : HavokData<HKLib.hk2019.hkaiHeatMapViewer.Options> 
{
    public hkaiHeatMapViewerOptionsData(HavokType type, HKLib.hk2019.hkaiHeatMapViewer.Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_silhouetteCuttingHeatmap":
            case "silhouetteCuttingHeatmap":
            {
                if (instance.m_silhouetteCuttingHeatmap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCalculationHeatmap":
            case "clearanceCalculationHeatmap":
            {
                if (instance.m_clearanceCalculationHeatmap is not TGet castValue) return false;
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
            case "m_silhouetteCuttingHeatmap":
            case "silhouetteCuttingHeatmap":
            {
                if (value is not bool castValue) return false;
                instance.m_silhouetteCuttingHeatmap = castValue;
                return true;
            }
            case "m_clearanceCalculationHeatmap":
            case "clearanceCalculationHeatmap":
            {
                if (value is not bool castValue) return false;
                instance.m_clearanceCalculationHeatmap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
