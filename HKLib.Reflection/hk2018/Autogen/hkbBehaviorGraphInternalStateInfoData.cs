// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBehaviorGraphInternalStateInfoData : HavokData<hkbBehaviorGraphInternalStateInfo> 
{
    public hkbBehaviorGraphInternalStateInfoData(HavokType type, hkbBehaviorGraphInternalStateInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_internalState":
            case "internalState":
            {
                if (instance.m_internalState is null)
                {
                    return true;
                }
                if (instance.m_internalState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_auxiliaryNodeInfo":
            case "auxiliaryNodeInfo":
            {
                if (instance.m_auxiliaryNodeInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activeEventIds":
            case "activeEventIds":
            {
                if (instance.m_activeEventIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_activeVariableIds":
            case "activeVariableIds":
            {
                if (instance.m_activeVariableIds is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_internalState":
            case "internalState":
            {
                if (value is null)
                {
                    instance.m_internalState = default;
                    return true;
                }
                if (value is hkbBehaviorGraphInternalState castValue)
                {
                    instance.m_internalState = castValue;
                    return true;
                }
                return false;
            }
            case "m_auxiliaryNodeInfo":
            case "auxiliaryNodeInfo":
            {
                if (value is not List<hkbAuxiliaryNodeInfo?> castValue) return false;
                instance.m_auxiliaryNodeInfo = castValue;
                return true;
            }
            case "m_activeEventIds":
            case "activeEventIds":
            {
                if (value is not List<short> castValue) return false;
                instance.m_activeEventIds = castValue;
                return true;
            }
            case "m_activeVariableIds":
            case "activeVariableIds":
            {
                if (value is not List<short> castValue) return false;
                instance.m_activeVariableIds = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
