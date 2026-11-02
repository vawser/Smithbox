// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiViewerNodesLayerData : HavokData<HKLib.hk2019.hkaiViewerNodes.Layer> 
{
    public hkaiViewerNodesLayerData(HavokType type, HKLib.hk2019.hkaiViewerNodes.Layer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_defaultClearanceCeiling":
            case "defaultClearanceCeiling":
            {
                if (instance.m_defaultClearanceCeiling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteExtrusion":
            case "silhouetteExtrusion":
            {
                if (instance.m_silhouetteExtrusion is not TGet castValue) return false;
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
