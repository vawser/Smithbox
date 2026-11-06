// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbPoseStoringGeneratorOutputListenerStoredPoseData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbPoseStoringGeneratorOutputListener.StoredPose>
{
    public hkbPoseStoringGeneratorOutputListenerStoredPoseData(Havok.hk2014.hkbPoseStoringGeneratorOutputListener.StoredPose instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_node":
            case "node":
            {
                value = instance.m_node;
                return true;
            }
            case "m_pose":
            case "pose":
            {
                value = instance.m_pose;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                value = instance.m_worldFromModel;
                return true;
            }
            case "m_isPoseValid":
            case "isPoseValid":
            {
                value = instance.m_isPoseValid;
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
            case "m_node":
            case "node":
            {
                if (value is null)
                {
                    instance.m_node = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbNode castValue)
                {
                    instance.m_node = castValue;
                    return true;
                }
                return false;
            }
            case "m_pose":
            case "pose":
            {
                if (value is not List<Havok.hk2014.hkQsTransform> castValue) return false;
                instance.m_pose = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not Havok.hk2014.hkQsTransform castValue) return false;
                instance.m_worldFromModel = castValue;
                return true;
            }
            case "m_isPoseValid":
            case "isPoseValid":
            {
                if (value is not bool castValue) return false;
                instance.m_isPoseValid = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
