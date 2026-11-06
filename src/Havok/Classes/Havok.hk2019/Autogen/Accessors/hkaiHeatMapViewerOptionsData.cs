// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiHeatMapViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiHeatMapViewer.Options>
{
    public hkaiHeatMapViewerOptionsData(Havok.hk2019.hkaiHeatMapViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_silhouetteCuttingHeatmap":
            case "silhouetteCuttingHeatmap":
            {
                value = instance.m_silhouetteCuttingHeatmap;
                return true;
            }
            case "m_clearanceCalculationHeatmap":
            case "clearanceCalculationHeatmap":
            {
                value = instance.m_clearanceCalculationHeatmap;
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
