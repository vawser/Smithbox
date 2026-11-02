// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkReflectVersionPatchInfoData : HavokData<HKLib.hk2019.hkReflect.Version.PatchInfo> 
{
    public hkReflectVersionPatchInfoData(HavokType type, HKLib.hk2019.hkReflect.Version.PatchInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oldName":
            case "oldName":
            {
                if (instance.m_oldName is null)
                {
                    return true;
                }
                if (instance.m_oldName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_newName":
            case "newName":
            {
                if (instance.m_newName is null)
                {
                    return true;
                }
                if (instance.m_newName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_oldVersion":
            case "oldVersion":
            {
                if (instance.m_oldVersion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newVersion":
            case "newVersion":
            {
                if (instance.m_newVersion is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_component":
            case "component":
            {
                if (instance.m_component is null)
                {
                    return true;
                }
                if (instance.m_component is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_numComponent":
            case "numComponent":
            {
                if (instance.m_numComponent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isNewPatch":
            case "isNewPatch":
            {
                if (instance.m_isNewPatch is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isCombine":
            case "isCombine":
            {
                if (instance.m_isCombine is not TGet castValue) return false;
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
            case "m_oldName":
            case "oldName":
            {
                if (value is null)
                {
                    instance.m_oldName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_oldName = castValue;
                    return true;
                }
                return false;
            }
            case "m_newName":
            case "newName":
            {
                if (value is null)
                {
                    instance.m_newName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_newName = castValue;
                    return true;
                }
                return false;
            }
            case "m_oldVersion":
            case "oldVersion":
            {
                if (value is not int castValue) return false;
                instance.m_oldVersion = castValue;
                return true;
            }
            case "m_newVersion":
            case "newVersion":
            {
                if (value is not int castValue) return false;
                instance.m_newVersion = castValue;
                return true;
            }
            case "m_component":
            case "component":
            {
                if (value is null)
                {
                    instance.m_component = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkReflect.Version.PatchInfo.Component castValue)
                {
                    instance.m_component = castValue;
                    return true;
                }
                return false;
            }
            case "m_numComponent":
            case "numComponent":
            {
                if (value is not int castValue) return false;
                instance.m_numComponent = castValue;
                return true;
            }
            case "m_isNewPatch":
            case "isNewPatch":
            {
                if (value is not bool castValue) return false;
                instance.m_isNewPatch = castValue;
                return true;
            }
            case "m_isCombine":
            case "isCombine":
            {
                if (value is not bool castValue) return false;
                instance.m_isCombine = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
