// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterStringDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterStringData>
{
    public hkbCharacterStringDataData(Havok.hk2014.hkbCharacterStringData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skinNames":
            case "skinNames":
            {
                value = instance.m_skinNames;
                return true;
            }
            case "m_boneAttachmentNames":
            case "boneAttachmentNames":
            {
                value = instance.m_boneAttachmentNames;
                return true;
            }
            case "m_animationBundleNameData":
            case "animationBundleNameData":
            {
                value = instance.m_animationBundleNameData;
                return true;
            }
            case "m_animationBundleFilenameData":
            case "animationBundleFilenameData":
            {
                value = instance.m_animationBundleFilenameData;
                return true;
            }
            case "m_characterPropertyNames":
            case "characterPropertyNames":
            {
                value = instance.m_characterPropertyNames;
                return true;
            }
            case "m_retargetingSkeletonMapperFilenames":
            case "retargetingSkeletonMapperFilenames":
            {
                value = instance.m_retargetingSkeletonMapperFilenames;
                return true;
            }
            case "m_lodNames":
            case "lodNames":
            {
                value = instance.m_lodNames;
                return true;
            }
            case "m_mirroredSyncPointSubstringsA":
            case "mirroredSyncPointSubstringsA":
            {
                value = instance.m_mirroredSyncPointSubstringsA;
                return true;
            }
            case "m_mirroredSyncPointSubstringsB":
            case "mirroredSyncPointSubstringsB":
            {
                value = instance.m_mirroredSyncPointSubstringsB;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_rigName":
            case "rigName":
            {
                value = instance.m_rigName;
                return true;
            }
            case "m_ragdollName":
            case "ragdollName":
            {
                value = instance.m_ragdollName;
                return true;
            }
            case "m_behaviorFilename":
            case "behaviorFilename":
            {
                value = instance.m_behaviorFilename;
                return true;
            }
            case "m_luaScriptOnCharacterActivated":
            case "luaScriptOnCharacterActivated":
            {
                value = instance.m_luaScriptOnCharacterActivated;
                return true;
            }
            case "m_luaScriptOnCharacterDeactivated":
            case "luaScriptOnCharacterDeactivated":
            {
                value = instance.m_luaScriptOnCharacterDeactivated;
                return true;
            }
            case "m_luaFiles":
            case "luaFiles":
            {
                value = instance.m_luaFiles;
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
            case "m_skinNames":
            case "skinNames":
            {
                if (value is not List<Havok.hk2014.hkbCharacterStringData.FileNameMeshNamePair> castValue) return false;
                instance.m_skinNames = castValue;
                return true;
            }
            case "m_boneAttachmentNames":
            case "boneAttachmentNames":
            {
                if (value is not List<Havok.hk2014.hkbCharacterStringData.FileNameMeshNamePair> castValue) return false;
                instance.m_boneAttachmentNames = castValue;
                return true;
            }
            case "m_animationBundleNameData":
            case "animationBundleNameData":
            {
                if (value is not List<Havok.hk2014.hkbAssetBundleStringData> castValue) return false;
                instance.m_animationBundleNameData = castValue;
                return true;
            }
            case "m_animationBundleFilenameData":
            case "animationBundleFilenameData":
            {
                if (value is not List<Havok.hk2014.hkbAssetBundleStringData> castValue) return false;
                instance.m_animationBundleFilenameData = castValue;
                return true;
            }
            case "m_characterPropertyNames":
            case "characterPropertyNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_characterPropertyNames = castValue;
                return true;
            }
            case "m_retargetingSkeletonMapperFilenames":
            case "retargetingSkeletonMapperFilenames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_retargetingSkeletonMapperFilenames = castValue;
                return true;
            }
            case "m_lodNames":
            case "lodNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_lodNames = castValue;
                return true;
            }
            case "m_mirroredSyncPointSubstringsA":
            case "mirroredSyncPointSubstringsA":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_mirroredSyncPointSubstringsA = castValue;
                return true;
            }
            case "m_mirroredSyncPointSubstringsB":
            case "mirroredSyncPointSubstringsB":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_mirroredSyncPointSubstringsB = castValue;
                return true;
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
            case "m_rigName":
            case "rigName":
            {
                if (value is null)
                {
                    instance.m_rigName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_rigName = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollName":
            case "ragdollName":
            {
                if (value is null)
                {
                    instance.m_ragdollName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_ragdollName = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorFilename":
            case "behaviorFilename":
            {
                if (value is null)
                {
                    instance.m_behaviorFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_behaviorFilename = castValue;
                    return true;
                }
                return false;
            }
            case "m_luaScriptOnCharacterActivated":
            case "luaScriptOnCharacterActivated":
            {
                if (value is null)
                {
                    instance.m_luaScriptOnCharacterActivated = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_luaScriptOnCharacterActivated = castValue;
                    return true;
                }
                return false;
            }
            case "m_luaScriptOnCharacterDeactivated":
            case "luaScriptOnCharacterDeactivated":
            {
                if (value is null)
                {
                    instance.m_luaScriptOnCharacterDeactivated = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_luaScriptOnCharacterDeactivated = castValue;
                    return true;
                }
                return false;
            }
            case "m_luaFiles":
            case "luaFiles":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_luaFiles = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
