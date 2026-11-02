// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavigatorViewerOptionsData : HavokData<HKLib.hk2019.hkaiNavigatorViewer.Options> 
{
    public hkaiNavigatorViewerOptionsData(HavokType type, HKLib.hk2019.hkaiNavigatorViewer.Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_showPositions":
            case "showPositions":
            {
                if (instance.m_showPositions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showGoals":
            case "showGoals":
            {
                if (instance.m_showGoals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showIdealDirections":
            case "showIdealDirections":
            {
                if (instance.m_showIdealDirections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showEvents":
            case "showEvents":
            {
                if (instance.m_showEvents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showPaths":
            case "showPaths":
            {
                if (instance.m_showPaths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_showGates":
            case "showGates":
            {
                if (instance.m_showGates is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_displayOffset":
            case "displayOffset":
            {
                if (instance.m_displayOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventsOffsetHeight":
            case "eventsOffsetHeight":
            {
                if (instance.m_eventsOffsetHeight is not TGet castValue) return false;
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
