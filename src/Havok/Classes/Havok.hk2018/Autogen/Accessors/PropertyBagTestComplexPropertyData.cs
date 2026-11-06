// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class PropertyBagTestComplexPropertyData : Havok.Formats.HavokFieldAccess<Havok.hk2018.PropertyBagTest.ComplexProperty>
{
    public PropertyBagTestComplexPropertyData(Havok.hk2018.PropertyBagTest.ComplexProperty instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_object":
            case "object":
            {
                value = instance.m_object;
                return true;
            }
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
                return true;
            }
            case "m_afterReflectNewCalled":
            case "afterReflectNewCalled":
            {
                value = instance.m_afterReflectNewCalled;
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
            case "m_object":
            case "object":
            {
                if (value is null)
                {
                    instance.m_object = default;
                    return true;
                }
                if (value is Havok.hk2018.hkReferencedObject castValue)
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
                if (value is Havok.hk2018.hkReferencedObject castValue)
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
