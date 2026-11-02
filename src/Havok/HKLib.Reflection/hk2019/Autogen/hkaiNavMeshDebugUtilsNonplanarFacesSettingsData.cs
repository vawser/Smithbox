// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshDebugUtilsNonplanarFacesSettingsData : HavokData<HKLib.hk2019.hkaiNavMeshDebugUtils.NonplanarFacesSettings> 
{
    public hkaiNavMeshDebugUtilsNonplanarFacesSettingsData(HavokType type, HKLib.hk2019.hkaiNavMeshDebugUtils.NonplanarFacesSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_highlightNonplanarFaces":
            case "highlightNonplanarFaces":
            {
                if (instance.m_highlightNonplanarFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawNonplanarity":
            case "drawNonplanarity":
            {
                if (instance.m_drawNonplanarity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_scaleMultiplier":
            case "scaleMultiplier":
            {
                if (instance.m_scaleMultiplier is not TGet castValue) return false;
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
            case "m_highlightNonplanarFaces":
            case "highlightNonplanarFaces":
            {
                if (value is not bool castValue) return false;
                instance.m_highlightNonplanarFaces = castValue;
                return true;
            }
            case "m_drawNonplanarity":
            case "drawNonplanarity":
            {
                if (value is not bool castValue) return false;
                instance.m_drawNonplanarity = castValue;
                return true;
            }
            case "m_scaleMultiplier":
            case "scaleMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_scaleMultiplier = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
