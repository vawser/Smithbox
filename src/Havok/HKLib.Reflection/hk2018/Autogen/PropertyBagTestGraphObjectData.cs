// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.PropertyBagTest;

namespace HKLib.Reflection.hk2018;

internal class PropertyBagTestGraphObjectData : HavokData<GraphObject> 
{
    public PropertyBagTestGraphObjectData(HavokType type, GraphObject instance) : base(type, instance) {}

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
            case "m_child":
            case "child":
            {
                if (instance.m_child is null)
                {
                    return true;
                }
                if (instance.m_child is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is GraphObject castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
