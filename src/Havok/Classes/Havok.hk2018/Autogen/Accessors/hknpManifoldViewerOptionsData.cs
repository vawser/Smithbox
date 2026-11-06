// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpManifoldViewerOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpManifoldViewer.Options>
{
    public hknpManifoldViewerOptionsData(Havok.hk2018.hknpManifoldViewer.Options instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_drawContacts":
            case "drawContacts":
            {
                value = instance.m_drawContacts;
                return true;
            }
            case "m_drawUnweldedContacts":
            case "drawUnweldedContacts":
            {
                value = instance.m_drawUnweldedContacts;
                return true;
            }
            case "m_drawManifoldBorders":
            case "drawManifoldBorders":
            {
                value = instance.m_drawManifoldBorders;
                return true;
            }
            case "m_drawManifoldIds":
            case "drawManifoldIds":
            {
                value = instance.m_drawManifoldIds;
                return true;
            }
            case "m_drawImpulses":
            case "drawImpulses":
            {
                value = instance.m_drawImpulses;
                return true;
            }
            case "m_impulseScale":
            case "impulseScale":
            {
                value = instance.m_impulseScale;
                return true;
            }
            case "m_advancedWeldingInfo":
            case "advancedWeldingInfo":
            {
                value = instance.m_advancedWeldingInfo;
                return true;
            }
            case "m_addCountersToTimerTree":
            case "addCountersToTimerTree":
            {
                value = instance.m_addCountersToTimerTree;
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
            case "m_drawContacts":
            case "drawContacts":
            {
                if (value is not bool castValue) return false;
                instance.m_drawContacts = castValue;
                return true;
            }
            case "m_drawUnweldedContacts":
            case "drawUnweldedContacts":
            {
                if (value is not bool castValue) return false;
                instance.m_drawUnweldedContacts = castValue;
                return true;
            }
            case "m_drawManifoldBorders":
            case "drawManifoldBorders":
            {
                if (value is not bool castValue) return false;
                instance.m_drawManifoldBorders = castValue;
                return true;
            }
            case "m_drawManifoldIds":
            case "drawManifoldIds":
            {
                if (value is not bool castValue) return false;
                instance.m_drawManifoldIds = castValue;
                return true;
            }
            case "m_drawImpulses":
            case "drawImpulses":
            {
                if (value is not bool castValue) return false;
                instance.m_drawImpulses = castValue;
                return true;
            }
            case "m_impulseScale":
            case "impulseScale":
            {
                if (value is not float castValue) return false;
                instance.m_impulseScale = castValue;
                return true;
            }
            case "m_advancedWeldingInfo":
            case "advancedWeldingInfo":
            {
                if (value is not bool castValue) return false;
                instance.m_advancedWeldingInfo = castValue;
                return true;
            }
            case "m_addCountersToTimerTree":
            case "addCountersToTimerTree":
            {
                if (value is not bool castValue) return false;
                instance.m_addCountersToTimerTree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
