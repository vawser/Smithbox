// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkReflectDetailsData : HavokData<ReflectDetails> 
{
    public hkReflectDetailsData(HavokType type, ReflectDetails instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_parents":
            case "parents":
            {
                if (instance.m_parents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inheritance":
            case "inheritance":
            {
                if (instance.m_inheritance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_specials":
            case "specials":
            {
                if (instance.m_specials is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fields":
            case "fields":
            {
                if (instance.m_fields is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_methods":
            case "methods":
            {
                if (instance.m_methods is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowMultipleDataInheritance":
            case "allowMultipleDataInheritance":
            {
                if (instance.m_allowMultipleDataInheritance is not TGet castValue) return false;
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
