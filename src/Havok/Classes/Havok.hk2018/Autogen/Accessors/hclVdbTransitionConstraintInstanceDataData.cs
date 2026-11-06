// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVdbTransitionConstraintInstanceDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVdbTransitionConstraintInstanceData>
{
    public hclVdbTransitionConstraintInstanceDataData(Havok.hk2018.hclVdbTransitionConstraintInstanceData instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            default:
            return false;
        }
    }

}
