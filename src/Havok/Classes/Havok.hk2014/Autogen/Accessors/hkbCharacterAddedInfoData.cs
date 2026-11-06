// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterAddedInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterAddedInfo>
{
    public hkbCharacterAddedInfoData(Havok.hk2014.hkbCharacterAddedInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_instanceName":
            case "instanceName":
            {
                value = instance.m_instanceName;
                return true;
            }
            case "m_templateName":
            case "templateName":
            {
                value = instance.m_templateName;
                return true;
            }
            case "m_fullPathToProject":
            case "fullPathToProject":
            {
                value = instance.m_fullPathToProject;
                return true;
            }
            case "m_localScriptsPath":
            case "localScriptsPath":
            {
                value = instance.m_localScriptsPath;
                return true;
            }
            case "m_remoteScriptsPath":
            case "remoteScriptsPath":
            {
                value = instance.m_remoteScriptsPath;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                value = instance.m_skeleton;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                value = instance.m_worldFromModel;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                value = instance.m_poseModelSpace;
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
                if (value is Havok.hk2014.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not Havok.hk2014.hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (value is not List<Havok.hk2014.hkQsTransform> castValue) return false;
                instance.m_poseModelSpace = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
