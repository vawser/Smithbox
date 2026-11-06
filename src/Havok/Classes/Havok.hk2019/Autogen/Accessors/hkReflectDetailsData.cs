// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkReflectDetailsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hk.ReflectDetails>
{
    public hkReflectDetailsData(Havok.hk2019.hk.ReflectDetails instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_parents":
            case "parents":
            {
                value = instance.m_parents;
                return true;
            }
            case "m_inheritance":
            case "inheritance":
            {
                value = instance.m_inheritance;
                return true;
            }
            case "m_specials":
            case "specials":
            {
                value = instance.m_specials;
                return true;
            }
            case "m_fields":
            case "fields":
            {
                value = instance.m_fields;
                return true;
            }
            case "m_methods":
            case "methods":
            {
                value = instance.m_methods;
                return true;
            }
            case "m_allowMultipleDataInheritance":
            case "allowMultipleDataInheritance":
            {
                value = instance.m_allowMultipleDataInheritance;
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
            case "m_parents":
            case "parents":
            {
                if (value is not bool castValue) return false;
                instance.m_parents = castValue;
                return true;
            }
            case "m_inheritance":
            case "inheritance":
            {
                if (value is not bool castValue) return false;
                instance.m_inheritance = castValue;
                return true;
            }
            case "m_specials":
            case "specials":
            {
                if (value is not bool castValue) return false;
                instance.m_specials = castValue;
                return true;
            }
            case "m_fields":
            case "fields":
            {
                if (value is not bool castValue) return false;
                instance.m_fields = castValue;
                return true;
            }
            case "m_methods":
            case "methods":
            {
                if (value is not bool castValue) return false;
                instance.m_methods = castValue;
                return true;
            }
            case "m_allowMultipleDataInheritance":
            case "allowMultipleDataInheritance":
            {
                if (value is not bool castValue) return false;
                instance.m_allowMultipleDataInheritance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
