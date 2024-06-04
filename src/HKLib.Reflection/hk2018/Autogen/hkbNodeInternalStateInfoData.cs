// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbNodeInternalStateInfoData : HavokData<hkbNodeInternalStateInfo> 
{
    public hkbNodeInternalStateInfoData(HavokType type, hkbNodeInternalStateInfo instance) : base(type, instance) {}

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
            case "m_syncInfo":
            case "syncInfo":
            {
                if (instance.m_syncInfo is null)
                {
                    return true;
                }
                if (instance.m_syncInfo is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_nodeId":
            case "nodeId":
            {
                if (instance.m_nodeId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasActivateBeenCalled":
            case "hasActivateBeenCalled":
            {
                if (instance.m_hasActivateBeenCalled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isModifierEnabled":
            case "isModifierEnabled":
            {
                if (instance.m_isModifierEnabled is not TGet castValue) return false;
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
            case "m_syncInfo":
            case "syncInfo":
            {
                if (value is null)
                {
                    instance.m_syncInfo = default;
                    return true;
                }
                if (value is hkbReferencedGeneratorSyncInfo castValue)
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
                if (value is hkReferencedObject castValue)
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
