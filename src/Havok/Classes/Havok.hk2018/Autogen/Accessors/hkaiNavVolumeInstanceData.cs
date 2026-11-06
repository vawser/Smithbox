// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumeInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolumeInstance>
{
    public hkaiNavVolumeInstanceData(Havok.hk2018.hkaiNavVolumeInstance instance) : base(instance) { }

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
            case "m_instanceData":
            case "instanceData":
            {
                if (value is not Havok.hk2018.hkaiCopyOnWritePtr<Havok.hk2018.hkaiNavVolumeInstanceData> castValue) return false;
                instance.m_instanceData = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Havok.hk2018.hkAxialTransform castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
