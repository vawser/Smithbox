// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeInstance>
{
    public hkaiNavVolumeInstanceData(Havok.hk2019.hkaiNavVolumeInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_instanceData":
            case "instanceData":
            {
                value = instance.m_instanceData;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_used":
            case "used":
            {
                value = instance.m_used;
                return true;
            }
            case "m_mediator":
            case "mediator":
            {
                value = instance.m_mediator;
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
                if (value is not Havok.hk2019.hkaiCopyOnWritePtr<Havok.hk2019.hkaiNavVolumeInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2019.hkAxialTransform castValue) return false;
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
                if (value is Havok.hk2019.hkaiNavVolumeInstanceQueryMediator castValue)
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
