// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkExposeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.Expose>
{
    public hkExposeData(Havok.hk2018.hk.Expose instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_access":
            case "access":
            {
                value = instance.m_access;
                return true;
            }
            case "m_scope":
            case "scope":
            {
                value = instance.m_scope;
                return true;
            }
            case "m_valueType":
            case "valueType":
            {
                value = instance.m_valueType;
                return true;
            }
            case "m_getter":
            case "getter":
            {
                value = instance.m_getter;
                return true;
            }
            case "m_setter":
            case "setter":
            {
                value = instance.m_setter;
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
            case "m_access":
            case "access":
            {
                if (value is Havok.hk2018.hk.ExposeBase.AccessValues castValue)
                {
                    instance.m_access = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_access = (Havok.hk2018.hk.ExposeBase.AccessValues)byteValue;
                    return true;
                }
                return false;
            }
            case "m_scope":
            case "scope":
            {
                if (value is Havok.hk2018.hk.ExposeBase.ScopeValues castValue)
                {
                    instance.m_scope = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_scope = (Havok.hk2018.hk.ExposeBase.ScopeValues)byteValue;
                    return true;
                }
                return false;
            }
            case "m_valueType":
            case "valueType":
            {
                if (value is null)
                {
                    instance.m_valueType = default;
                    return true;
                }
                if (value is Havok.hk2018.IHavokObject castValue)
                {
                    instance.m_valueType = castValue;
                    return true;
                }
                return false;
            }
            case "m_getter":
            case "getter":
            {
                if (value is null)
                {
                    instance.m_getter = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_getter = castValue;
                    return true;
                }
                return false;
            }
            case "m_setter":
            case "setter":
            {
                if (value is null)
                {
                    instance.m_setter = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_setter = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
