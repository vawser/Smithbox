// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterStringDataData : HavokData<hkbCharacterStringData> 
{
    public hkbCharacterStringDataData(HavokType type, hkbCharacterStringData instance) : base(type, instance) {}

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
            case "m_skinNames":
            case "skinNames":
            {
                if (instance.m_skinNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneAttachmentNames":
            case "boneAttachmentNames":
            {
                if (instance.m_boneAttachmentNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationBundleNameData":
            case "animationBundleNameData":
            {
                if (instance.m_animationBundleNameData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationBundleFilenameData":
            case "animationBundleFilenameData":
            {
                if (instance.m_animationBundleFilenameData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterPropertyNames":
            case "characterPropertyNames":
            {
                if (instance.m_characterPropertyNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_retargetingSkeletonMapperFilenames":
            case "retargetingSkeletonMapperFilenames":
            {
                if (instance.m_retargetingSkeletonMapperFilenames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lodNames":
            case "lodNames":
            {
                if (instance.m_lodNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mirroredSyncPointSubstringsA":
            case "mirroredSyncPointSubstringsA":
            {
                if (instance.m_mirroredSyncPointSubstringsA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mirroredSyncPointSubstringsB":
            case "mirroredSyncPointSubstringsB":
            {
                if (instance.m_mirroredSyncPointSubstringsB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_rigName":
            case "rigName":
            {
                if (instance.m_rigName is null)
                {
                    return true;
                }
                if (instance.m_rigName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollName":
            case "ragdollName":
            {
                if (instance.m_ragdollName is null)
                {
                    return true;
                }
                if (instance.m_ragdollName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorFilename":
            case "behaviorFilename":
            {
                if (instance.m_behaviorFilename is null)
                {
                    return true;
                }
                if (instance.m_behaviorFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_luaScriptOnCharacterActivated":
            case "luaScriptOnCharacterActivated":
            {
                if (instance.m_luaScriptOnCharacterActivated is null)
                {
                    return true;
                }
                if (instance.m_luaScriptOnCharacterActivated is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_luaScriptOnCharacterDeactivated":
            case "luaScriptOnCharacterDeactivated":
            {
                if (instance.m_luaScriptOnCharacterDeactivated is null)
                {
                    return true;
                }
                if (instance.m_luaScriptOnCharacterDeactivated is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_luaFiles":
            case "luaFiles":
            {
                if (instance.m_luaFiles is not TGet castValue) return false;
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
            case "m_skinNames":
            case "skinNames":
            {
                if (value is not List<hkbCharacterStringData.FileNameMeshNamePair> castValue) return false;
                instance.m_skinNames = castValue;
                return true;
            }
            case "m_boneAttachmentNames":
            case "boneAttachmentNames":
            {
                if (value is not List<hkbCharacterStringData.FileNameMeshNamePair> castValue) return false;
                instance.m_boneAttachmentNames = castValue;
                return true;
            }
            case "m_animationBundleNameData":
            case "animationBundleNameData":
            {
                if (value is not List<hkbAssetBundleStringData> castValue) return false;
                instance.m_animationBundleNameData = castValue;
                return true;
            }
            case "m_animationBundleFilenameData":
            case "animationBundleFilenameData":
            {
                if (value is not List<hkbAssetBundleStringData> castValue) return false;
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
