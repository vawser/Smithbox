// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPathRequestBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPathRequestBase>
{
    public hkaiPathRequestBaseData(Havok.hk2018.hkaiPathRequestBase instance) : base(instance) { }

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
            case "m_queueId":
            case "queueId":
            {
                value = instance.m_queueId;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_requestState":
            case "requestState":
            {
                value = instance.m_requestState;
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
            case "m_queueId":
            case "queueId":
            {
                if (value is not Havok.hk2018.hkHandle<byte> castValue) return false;
                instance.m_queueId = castValue;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                if (value is not int castValue) return false;
                instance.m_priority = castValue;
                return true;
            }
            case "m_requestState":
            case "requestState":
            {
                if (value is not Havok.hk2018.hkAtomic.Variable<Havok.hk2018.hkaiPathRequestBase.RequestState> castValue) return false;
                instance.m_requestState = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
