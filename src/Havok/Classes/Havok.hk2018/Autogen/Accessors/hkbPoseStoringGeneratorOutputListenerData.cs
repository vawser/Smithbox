// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbPoseStoringGeneratorOutputListenerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbPoseStoringGeneratorOutputListener>
{
    public hkbPoseStoringGeneratorOutputListenerData(Havok.hk2018.hkbPoseStoringGeneratorOutputListener instance) : base(instance) { }

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
            case "m_storedPoses":
            case "storedPoses":
            {
                value = instance.m_storedPoses;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                value = instance.m_dirty;
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
            case "m_storedPoses":
            case "storedPoses":
            {
                if (value is not List<Havok.hk2018.hkbPoseStoringGeneratorOutputListener.StoredPose?> castValue) return false;
                instance.m_storedPoses = castValue;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                if (value is not bool castValue) return false;
                instance.m_dirty = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
