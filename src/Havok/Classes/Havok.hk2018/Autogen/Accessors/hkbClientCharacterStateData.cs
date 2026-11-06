// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbClientCharacterStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbClientCharacterState>
{
    public hkbClientCharacterStateData(Havok.hk2018.hkbClientCharacterState instance) : base(instance) { }

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
            case "m_deformableSkinIds":
            case "deformableSkinIds":
            {
                value = instance.m_deformableSkinIds;
                return true;
            }
            case "m_rigidSkinIds":
            case "rigidSkinIds":
            {
                value = instance.m_rigidSkinIds;
                return true;
            }
            case "m_externalEventIds":
            case "externalEventIds":
            {
                value = instance.m_externalEventIds;
                return true;
            }
            case "m_auxiliaryInfo":
            case "auxiliaryInfo":
            {
                value = instance.m_auxiliaryInfo;
                return true;
            }
            case "m_activeEventIds":
            case "activeEventIds":
            {
                value = instance.m_activeEventIds;
                return true;
            }
            case "m_activeVariableIds":
            case "activeVariableIds":
            {
                value = instance.m_activeVariableIds;
                return true;
            }
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
            case "m_behaviorData":
            case "behaviorData":
            {
                value = instance.m_behaviorData;
                return true;
            }
            case "m_behaviorInternalState":
            case "behaviorInternalState":
            {
                value = instance.m_behaviorInternalState;
                return true;
            }
            case "m_visible":
            case "visible":
            {
                value = instance.m_visible;
                return true;
            }
            case "m_elapsedSimulationTime":
            case "elapsedSimulationTime":
            {
                value = instance.m_elapsedSimulationTime;
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
            case "m_rigidAttachmentTransforms":
            case "rigidAttachmentTransforms":
            {
                value = instance.m_rigidAttachmentTransforms;
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
            case "m_deformableSkinIds":
            case "deformableSkinIds":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_deformableSkinIds = castValue;
                return true;
            }
            case "m_rigidSkinIds":
            case "rigidSkinIds":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_rigidSkinIds = castValue;
                return true;
            }
            case "m_externalEventIds":
            case "externalEventIds":
            {
                if (value is not List<short> castValue) return false;
                instance.m_externalEventIds = castValue;
                return true;
            }
            case "m_auxiliaryInfo":
            case "auxiliaryInfo":
            {
                if (value is not List<Havok.hk2018.hkbAuxiliaryNodeInfo?> castValue) return false;
                instance.m_auxiliaryInfo = castValue;
                return true;
            }
            case "m_activeEventIds":
            case "activeEventIds":
            {
                if (value is not List<short> castValue) return false;
                instance.m_activeEventIds = castValue;
                return true;
            }
            case "m_activeVariableIds":
            case "activeVariableIds":
            {
                if (value is not List<short> castValue) return false;
                instance.m_activeVariableIds = castValue;
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
            case "m_behaviorData":
            case "behaviorData":
            {
                if (value is null)
                {
                    instance.m_behaviorData = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbBehaviorGraphData castValue)
                {
                    instance.m_behaviorData = castValue;
                    return true;
                }
                return false;
            }
            case "m_behaviorInternalState":
            case "behaviorInternalState":
            {
                if (value is null)
                {
                    instance.m_behaviorInternalState = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbBehaviorGraphInternalState castValue)
                {
                    instance.m_behaviorInternalState = castValue;
                    return true;
                }
                return false;
            }
            case "m_visible":
            case "visible":
            {
                if (value is not bool castValue) return false;
                instance.m_visible = castValue;
                return true;
            }
            case "m_elapsedSimulationTime":
            case "elapsedSimulationTime":
            {
                if (value is not float castValue) return false;
                instance.m_elapsedSimulationTime = castValue;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not Havok.hk2018.hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_poseModelSpace":
            case "poseModelSpace":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_poseModelSpace = castValue;
                return true;
            }
            case "m_rigidAttachmentTransforms":
            case "rigidAttachmentTransforms":
            {
                if (value is not List<Havok.hk2018.hkQsTransform> castValue) return false;
                instance.m_rigidAttachmentTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
