// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkp6DofConstraintDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkp6DofConstraintData>
{
    private static readonly System.Reflection.FieldInfo _atomToCompiledAtomOffsetInfo = typeof(Havok.hk2018.hkp6DofConstraintData).GetField("m_atomToCompiledAtomOffset")!;
    private static readonly System.Reflection.FieldInfo _resultToRuntimeInfo = typeof(Havok.hk2018.hkp6DofConstraintData).GetField("m_resultToRuntime")!;
    public hkp6DofConstraintDataData(Havok.hk2018.hkp6DofConstraintData instance) : base(instance) { }

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
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_blueprints":
            case "blueprints":
            {
                value = instance.m_blueprints;
                return true;
            }
            case "m_isDirty":
            case "isDirty":
            {
                value = instance.m_isDirty;
                return true;
            }
            case "m_numRuntimeElements":
            case "numRuntimeElements":
            {
                value = instance.m_numRuntimeElements;
                return true;
            }
            case "m_atomToCompiledAtomOffset":
            case "atomToCompiledAtomOffset":
            {
                value = instance.m_atomToCompiledAtomOffset;
                return true;
            }
            case "m_resultToRuntime":
            case "resultToRuntime":
            {
                value = instance.m_resultToRuntime;
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
                if (value is not Havok.hk2018.hkp6DofConstraintData.Blueprints castValue) return false;
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
