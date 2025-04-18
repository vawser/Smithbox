// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterAddedInfoData : HavokData<hkbCharacterAddedInfo> 
{
    public hkbCharacterAddedInfoData(HavokType type, hkbCharacterAddedInfo instance) : base(type, instance) {}

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
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceName":
            case "instanceName":
            {
                if (instance.m_instanceName is null)
                {
                    return true;
                }
                if (instance.m_instanceName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_templateName":
            case "templateName":
            {
                if (instance.m_templateName is null)
                {
                    return true;
                }
                if (instance.m_templateName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_fullPathToProject":
            case "fullPathToProject":
            {
                if (instance.m_fullPathToProject is null)
                {
                    return true;
                }
                if (instance.m_fullPathToProject is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_localScriptsPath":
            case "localScriptsPath":
            {
                if (instance.m_localScriptsPath is null)
                {
                    return true;
                }
                if (instance.m_localScriptsPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_remoteScriptsPath":
            case "remoteScriptsPath":
            {
                if (instance.m_remoteScriptsPath is null)
                {
                    return true;
                }
                if (instance.m_remoteScriptsPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_skeleton":
            case "skeleton":
            {
                if (instance.m_skeleton is null)
                {
                    return true;
                }
                if (instance.m_skeleton is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (instance.m_worldFromModel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (instance.m_poseModelSpace is not TGet castValue) return false;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_instanceName":
            case "instanceName":
            {
                if (value is null)
                {
                    instance.m_instanceName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_instanceName = castValue;
                    return true;
                }
                return false;
            }
            case "m_templateName":
            case "templateName":
            {
                if (value is null)
                {
                    instance.m_templateName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_templateName = castValue;
                    return true;
                }
                return false;
            }
            case "m_fullPathToProject":
            case "fullPathToProject":
            {
                if (value is null)
                {
                    instance.m_fullPathToProject = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_fullPathToProject = castValue;
                    return true;
                }
                return false;
            }
            case "m_localScriptsPath":
            case "localScriptsPath":
            {
                if (value is null)
                {
                    instance.m_localScriptsPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_localScriptsPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_remoteScriptsPath":
            case "remoteScriptsPath":
            {
                if (value is null)
                {
                    instance.m_remoteScriptsPath = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_remoteScriptsPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (value is not List<hkQsTransform> castValue) return false;
                instance.m_poseModelSpace = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
