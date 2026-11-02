// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumeInstanceData : HavokData<HKLib.hk2019.hkaiNavVolumeInstance> 
{
    public hkaiNavVolumeInstanceData(HavokType type, HKLib.hk2019.hkaiNavVolumeInstance instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (instance.m_instanceData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_used":
            case "used":
            {
                if (instance.m_used is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                if (instance.m_mediator is null)
                {
                    return true;
                }
                if (instance.m_mediator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                if (value is not HKLib.hk2019.hkaiCopyOnWritePtr<HKLib.hk2019.hkaiNavVolumeInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not HKLib.hk2019.hkAxialTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_used":
            case "used":
            {
                if (value is not bool castValue) return false;
                instance.m_used = castValue;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                if (value is null)
                {
                    instance.m_mediator = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiNavVolumeInstanceQueryMediator castValue)
                {
                    instance.m_mediator = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
