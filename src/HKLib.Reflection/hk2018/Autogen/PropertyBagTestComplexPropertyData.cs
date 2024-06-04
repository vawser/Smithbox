// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.PropertyBagTest;

namespace HKLib.Reflection.hk2018;

internal class PropertyBagTestComplexPropertyData : HavokData<ComplexProperty> 
{
    public PropertyBagTestComplexPropertyData(HavokType type, ComplexProperty instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_object":
            case "object":
            {
                if (instance.m_object is null)
                {
                    return true;
                }
                if (instance.m_object is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_parent":
            case "parent":
            {
                if (instance.m_parent is null)
                {
                    return true;
                }
                if (instance.m_parent is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_afterReflectNewCalled":
            case "afterReflectNewCalled":
            {
                if (instance.m_afterReflectNewCalled is not TGet castValue) return false;
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
            case "m_object":
            case "object":
            {
                if (value is null)
                {
                    instance.m_object = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_object = castValue;
                    return true;
                }
                return false;
            }
            case "m_parent":
            case "parent":
            {
                if (value is null)
                {
                    instance.m_parent = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_parent = castValue;
                    return true;
                }
                return false;
            }
            case "m_afterReflectNewCalled":
            case "afterReflectNewCalled":
            {
                if (value is not bool castValue) return false;
                instance.m_afterReflectNewCalled = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
