// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbNodeInternalStateInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbNodeInternalStateInfo>
{
    public hkbNodeInternalStateInfoData(Havok.hk2018.hkbNodeInternalStateInfo instance) : base(instance) { }

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
            case "m_syncInfo":
            case "syncInfo":
            {
                value = instance.m_syncInfo;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_internalState":
            case "internalState":
            {
                value = instance.m_internalState;
                return true;
            }
            case "m_nodeId":
            case "nodeId":
            {
                value = instance.m_nodeId;
                return true;
            }
            case "m_hasActivateBeenCalled":
            case "hasActivateBeenCalled":
            {
                value = instance.m_hasActivateBeenCalled;
                return true;
            }
            case "m_isModifierEnabled":
            case "isModifierEnabled":
            {
                value = instance.m_isModifierEnabled;
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
            case "m_syncInfo":
            case "syncInfo":
            {
                if (value is null)
                {
                    instance.m_syncInfo = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbReferencedGeneratorSyncInfo castValue)
                {
                    instance.m_syncInfo = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_internalState":
            case "internalState":
            {
                if (value is null)
                {
                    instance.m_internalState = default;
                    return true;
                }
                if (value is Havok.hk2018.hkReferencedObject castValue)
                {
                    instance.m_internalState = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeId":
            case "nodeId":
            {
                if (value is not ushort castValue) return false;
                instance.m_nodeId = castValue;
                return true;
            }
            case "m_hasActivateBeenCalled":
            case "hasActivateBeenCalled":
            {
                if (value is not bool castValue) return false;
                instance.m_hasActivateBeenCalled = castValue;
                return true;
            }
            case "m_isModifierEnabled":
            case "isModifierEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isModifierEnabled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
