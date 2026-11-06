// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiViewerNodesLayerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiViewerNodes.Layer>
{
    public hkaiViewerNodesLayerData(Havok.hk2019.hkaiViewerNodes.Layer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_defaultClearanceCeiling":
            case "defaultClearanceCeiling":
            {
                value = instance.m_defaultClearanceCeiling;
                return true;
            }
            case "m_silhouetteExtrusion":
            case "silhouetteExtrusion":
            {
                value = instance.m_silhouetteExtrusion;
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
            case "m_defaultClearanceCeiling":
            case "defaultClearanceCeiling":
            {
                if (value is not float castValue) return false;
                instance.m_defaultClearanceCeiling = castValue;
                return true;
            }
            case "m_silhouetteExtrusion":
            case "silhouetteExtrusion":
            {
                if (value is not float castValue) return false;
                instance.m_silhouetteExtrusion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
