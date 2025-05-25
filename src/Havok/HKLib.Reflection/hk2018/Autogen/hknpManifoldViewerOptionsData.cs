// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hknpManifoldViewer;

namespace HKLib.Reflection.hk2018;

internal class hknpManifoldViewerOptionsData : HavokData<Options> 
{
    public hknpManifoldViewerOptionsData(HavokType type, Options instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_drawContacts":
            case "drawContacts":
            {
                if (instance.m_drawContacts is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawUnweldedContacts":
            case "drawUnweldedContacts":
            {
                if (instance.m_drawUnweldedContacts is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawManifoldBorders":
            case "drawManifoldBorders":
            {
                if (instance.m_drawManifoldBorders is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawManifoldIds":
            case "drawManifoldIds":
            {
                if (instance.m_drawManifoldIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_drawImpulses":
            case "drawImpulses":
            {
                if (instance.m_drawImpulses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulseScale":
            case "impulseScale":
            {
                if (instance.m_impulseScale is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_advancedWeldingInfo":
            case "advancedWeldingInfo":
            {
                if (instance.m_advancedWeldingInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_addCountersToTimerTree":
            case "addCountersToTimerTree":
            {
                if (instance.m_addCountersToTimerTree is not TGet castValue) return false;
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
