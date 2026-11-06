// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkReflectVersionPatchInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkReflect.Version.PatchInfo>
{
    public hkReflectVersionPatchInfoData(Havok.hk2019.hkReflect.Version.PatchInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_oldName":
            case "oldName":
            {
                value = instance.m_oldName;
                return true;
            }
            case "m_newName":
            case "newName":
            {
                value = instance.m_newName;
                return true;
            }
            case "m_oldVersion":
            case "oldVersion":
            {
                value = instance.m_oldVersion;
                return true;
            }
            case "m_newVersion":
            case "newVersion":
            {
                value = instance.m_newVersion;
                return true;
            }
            case "m_component":
            case "component":
            {
                value = instance.m_component;
                return true;
            }
            case "m_numComponent":
            case "numComponent":
            {
                value = instance.m_numComponent;
                return true;
            }
            case "m_isNewPatch":
            case "isNewPatch":
            {
                value = instance.m_isNewPatch;
                return true;
            }
            case "m_isCombine":
            case "isCombine":
            {
                value = instance.m_isCombine;
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
                if (value is Havok.hk2019.hkReflect.Version.PatchInfo.Component castValue)
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
