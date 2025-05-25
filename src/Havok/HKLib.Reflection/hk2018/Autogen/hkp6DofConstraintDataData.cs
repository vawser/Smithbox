// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkp6DofConstraintDataData : HavokData<hkp6DofConstraintData> 
{
    private static readonly System.Reflection.FieldInfo _atomToCompiledAtomOffsetInfo = typeof(hkp6DofConstraintData).GetField("m_atomToCompiledAtomOffset")!;
    private static readonly System.Reflection.FieldInfo _resultToRuntimeInfo = typeof(hkp6DofConstraintData).GetField("m_resultToRuntime")!;
    public hkp6DofConstraintDataData(HavokType type, hkp6DofConstraintData instance) : base(type, instance) {}

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
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blueprints":
            case "blueprints":
            {
                if (instance.m_blueprints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isDirty":
            case "isDirty":
            {
                if (instance.m_isDirty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numRuntimeElements":
            case "numRuntimeElements":
            {
                if (instance.m_numRuntimeElements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_atomToCompiledAtomOffset":
            case "atomToCompiledAtomOffset":
            {
                if (instance.m_atomToCompiledAtomOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resultToRuntime":
            case "resultToRuntime":
            {
                if (instance.m_resultToRuntime is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_blueprints":
            case "blueprints":
            {
                if (value is not hkp6DofConstraintData.Blueprints castValue) return false;
                instance.m_blueprints = castValue;
                return true;
            }
            case "m_isDirty":
            case "isDirty":
            {
                if (value is not bool castValue) return false;
                instance.m_isDirty = castValue;
                return true;
            }
            case "m_numRuntimeElements":
            case "numRuntimeElements":
            {
                if (value is not int castValue) return false;
                instance.m_numRuntimeElements = castValue;
                return true;
            }
            case "m_atomToCompiledAtomOffset":
            case "atomToCompiledAtomOffset":
            {
                if (value is not int[] castValue || castValue.Length != 19) return false;
                try
                {
                    _atomToCompiledAtomOffsetInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_resultToRuntime":
            case "resultToRuntime":
            {
                if (value is not int[] castValue || castValue.Length != 19) return false;
                try
                {
                    _resultToRuntimeInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
