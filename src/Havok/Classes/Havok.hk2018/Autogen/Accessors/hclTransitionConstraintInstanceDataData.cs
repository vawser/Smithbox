// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclTransitionConstraintInstanceDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclTransitionConstraintInstanceData>
{
    public hclTransitionConstraintInstanceDataData(Havok.hk2018.hclTransitionConstraintInstanceData instance) : base(instance) { }

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
            case "m_state":
            case "state":
            {
                value = instance.m_state;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
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
            case "m_state":
            case "state":
            {
                if (value is Havok.hk2018.hclTransitionConstraintInstanceData.State castValue)
                {
                    instance.m_state = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_state = (Havok.hk2018.hclTransitionConstraintInstanceData.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
