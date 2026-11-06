// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshDebugUtilsNonplanarFacesSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshDebugUtils.NonplanarFacesSettings>
{
    public hkaiNavMeshDebugUtilsNonplanarFacesSettingsData(Havok.hk2018.hkaiNavMeshDebugUtils.NonplanarFacesSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_highlightNonplanarFaces":
            case "highlightNonplanarFaces":
            {
                value = instance.m_highlightNonplanarFaces;
                return true;
            }
            case "m_drawNonplanarity":
            case "drawNonplanarity":
            {
                value = instance.m_drawNonplanarity;
                return true;
            }
            case "m_scaleMultiplier":
            case "scaleMultiplier":
            {
                value = instance.m_scaleMultiplier;
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
