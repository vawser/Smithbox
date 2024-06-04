// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkExposeBaseData : HavokData<ExposeBase> 
{
    public hkExposeBaseData(HavokType type, ExposeBase instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_access":
            case "access":
            {
                if (instance.m_access is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_access is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_scope":
            case "scope":
            {
                if (instance.m_scope is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_scope is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_valueType":
            case "valueType":
            {
                if (instance.m_valueType is null)
                {
                    return true;
                }
                if (instance.m_valueType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_getter":
            case "getter":
            {
                if (instance.m_getter is null)
                {
                    return true;
                }
                if (instance.m_getter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_setter":
            case "setter":
            {
                if (instance.m_setter is null)
                {
                    return true;
                }
                if (instance.m_setter is TGet castValue)
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
            case "m_access":
            case "access":
            {
                if (value is ExposeBase.AccessValues castValue)
                {
                    instance.m_access = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_access = (ExposeBase.AccessValues)byteValue;
                    return true;
                }
                return false;
            }
            case "m_scope":
            case "scope":
            {
                if (value is ExposeBase.ScopeValues castValue)
                {
                    instance.m_scope = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_scope = (ExposeBase.ScopeValues)byteValue;
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
                if (value is IHavokObject castValue)
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
