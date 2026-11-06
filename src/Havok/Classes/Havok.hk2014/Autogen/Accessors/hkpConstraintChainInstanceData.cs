// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpConstraintChainInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpConstraintChainInstance>
{
    private static readonly System.Reflection.FieldInfo _entitiesInfo = typeof(Havok.hk2014.hkpConstraintChainInstance).GetField("m_entities")!;
    public hkpConstraintChainInstanceData(Havok.hk2014.hkpConstraintChainInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_constraintModifiers":
            case "constraintModifiers":
            {
                value = instance.m_constraintModifiers;
                return true;
            }
            case "m_entities":
            case "entities":
            {
                value = instance.m_entities;
                return true;
            }
            case "m_priority":
            case "priority":
            {
                value = instance.m_priority;
                return true;
            }
            case "m_wantRuntime":
            case "wantRuntime":
            {
                value = instance.m_wantRuntime;
                return true;
            }
            case "m_destructionRemapInfo":
            case "destructionRemapInfo":
            {
                value = instance.m_destructionRemapInfo;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_chainedEntities":
            case "chainedEntities":
            {
                value = instance.m_chainedEntities;
                return true;
            }
            case "m_action":
            case "action":
            {
                value = instance.m_action;
                return true;
            }
            case "m_chainConnectedness":
            case "chainConnectedness":
            {
                value = instance.m_chainConnectedness;
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
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpConstraintData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_constraintModifiers":
            case "constraintModifiers":
            {
                if (value is null)
                {
                    instance.m_constraintModifiers = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpModifierConstraintAtom castValue)
                {
                    instance.m_constraintModifiers = castValue;
                    return true;
                }
                return false;
            }
            case "m_entities":
            case "entities":
            {
                if (value is not Havok.hk2014.hkpEntity?[] castValue || castValue.Length != 2) return false;
                try
                {
                    _entitiesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_priority":
            case "priority":
            {
                if (value is Havok.hk2014.hkpConstraintInstance.ConstraintPriority castValue)
                {
                    instance.m_priority = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_priority = (Havok.hk2014.hkpConstraintInstance.ConstraintPriority)byteValue;
                    return true;
                }
                return false;
            }
            case "m_wantRuntime":
            case "wantRuntime":
            {
                if (value is not bool castValue) return false;
                instance.m_wantRuntime = castValue;
                return true;
            }
            case "m_destructionRemapInfo":
            case "destructionRemapInfo":
            {
                if (value is Havok.hk2014.hkpConstraintInstance.OnDestructionRemapInfo castValue)
                {
                    instance.m_destructionRemapInfo = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_destructionRemapInfo = (Havok.hk2014.hkpConstraintInstance.OnDestructionRemapInfo)byteValue;
                    return true;
                }
                return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_chainedEntities":
            case "chainedEntities":
            {
                if (value is not List<Havok.hk2014.hkpEntity?> castValue) return false;
                instance.m_chainedEntities = castValue;
                return true;
            }
            case "m_action":
            case "action":
            {
                if (value is null)
                {
                    instance.m_action = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpConstraintChainInstanceAction castValue)
                {
                    instance.m_action = castValue;
                    return true;
                }
                return false;
            }
            case "m_chainConnectedness":
            case "chainConnectedness":
            {
                if (value is not ulong castValue) return false;
                instance.m_chainConnectedness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
