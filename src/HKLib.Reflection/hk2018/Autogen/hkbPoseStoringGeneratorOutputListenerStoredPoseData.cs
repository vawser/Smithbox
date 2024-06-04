// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbPoseStoringGeneratorOutputListenerStoredPoseData : HavokData<hkbPoseStoringGeneratorOutputListener.StoredPose> 
{
    public hkbPoseStoringGeneratorOutputListenerStoredPoseData(HavokType type, hkbPoseStoringGeneratorOutputListener.StoredPose instance) : base(type, instance) {}

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
            case "m_node":
            case "node":
            {
                if (instance.m_node is null)
                {
                    return true;
                }
                if (instance.m_node is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pose":
            case "pose":
            {
                if (instance.m_pose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (instance.m_worldFromModel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isPoseValid":
            case "isPoseValid":
            {
                if (instance.m_isPoseValid is not TGet castValue) return false;
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
            case "m_node":
            case "node":
            {
                if (value is null)
                {
                    instance.m_node = default;
                    return true;
                }
                if (value is hkbNode castValue)
                {
                    instance.m_node = castValue;
                    return true;
                }
                return false;
            }
            case "m_pose":
            case "pose":
            {
                if (value is not List<hkQsTransform> castValue) return false;
                instance.m_pose = castValue;
                return true;
            }
            case "m_worldFromModel":
            case "worldFromModel":
            {
                if (value is not hkQsTransform castValue) return false;
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
