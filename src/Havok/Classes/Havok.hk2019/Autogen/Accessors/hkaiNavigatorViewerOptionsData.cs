// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavigatorViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavigatorViewer.Options>
{
    public hkaiNavigatorViewerOptionsData(Havok.hk2019.hkaiNavigatorViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showPositions":
            case "showPositions":
            {
                value = instance.m_showPositions;
                return true;
            }
            case "m_showGoals":
            case "showGoals":
            {
                value = instance.m_showGoals;
                return true;
            }
            case "m_showIdealDirections":
            case "showIdealDirections":
            {
                value = instance.m_showIdealDirections;
                return true;
            }
            case "m_showEvents":
            case "showEvents":
            {
                value = instance.m_showEvents;
                return true;
            }
            case "m_showPaths":
            case "showPaths":
            {
                value = instance.m_showPaths;
                return true;
            }
            case "m_showGates":
            case "showGates":
            {
                value = instance.m_showGates;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                value = instance.m_displayOffset;
                return true;
            }
            case "m_eventsOffsetHeight":
            case "eventsOffsetHeight":
            {
                value = instance.m_eventsOffsetHeight;
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
            case "m_showPositions":
            case "showPositions":
            {
                if (value is not bool castValue) return false;
                instance.m_showPositions = castValue;
                return true;
            }
            case "m_showGoals":
            case "showGoals":
            {
                if (value is not bool castValue) return false;
                instance.m_showGoals = castValue;
                return true;
            }
            case "m_showIdealDirections":
            case "showIdealDirections":
            {
                if (value is not bool castValue) return false;
                instance.m_showIdealDirections = castValue;
                return true;
            }
            case "m_showEvents":
            case "showEvents":
            {
                if (value is not bool castValue) return false;
                instance.m_showEvents = castValue;
                return true;
            }
            case "m_showPaths":
            case "showPaths":
            {
                if (value is not bool castValue) return false;
                instance.m_showPaths = castValue;
                return true;
            }
            case "m_showGates":
            case "showGates":
            {
                if (value is not bool castValue) return false;
                instance.m_showGates = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (value is not float castValue) return false;
                instance.m_displayOffset = castValue;
                return true;
            }
            case "m_eventsOffsetHeight":
            case "eventsOffsetHeight":
            {
                if (value is not float castValue) return false;
                instance.m_eventsOffsetHeight = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
