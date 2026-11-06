// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbBehaviorGraphInternalStateInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbBehaviorGraphInternalStateInfo>
{
    public hkbBehaviorGraphInternalStateInfoData(Havok.hk2018.hkbBehaviorGraphInternalStateInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_internalState":
            case "internalState":
            {
                value = instance.m_internalState;
                return true;
            }
            case "m_auxiliaryNodeInfo":
            case "auxiliaryNodeInfo":
            {
                value = instance.m_auxiliaryNodeInfo;
                return true;
            }
            case "m_activeEventIds":
            case "activeEventIds":
            {
                value = instance.m_activeEventIds;
                return true;
            }
            case "m_activeVariableIds":
            case "activeVariableIds":
            {
                value = instance.m_activeVariableIds;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hkbBehaviorGraphInternalState castValue)
                {
                    instance.m_internalState = castValue;
                    return true;
                }
                return false;
            }
            case "m_auxiliaryNodeInfo":
            case "auxiliaryNodeInfo":
            {
                if (value is not List<Havok.hk2018.hkbAuxiliaryNodeInfo?> castValue) return false;
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
