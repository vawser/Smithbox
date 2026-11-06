// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpDisplayBindingDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpDisplayBindingData>
{
    public hkpDisplayBindingDataData(Havok.hk2014.hkpDisplayBindingData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rigidBodyBindings":
            case "rigidBodyBindings":
            {
                value = instance.m_rigidBodyBindings;
                return true;
            }
            case "m_physicsSystemBindings":
            case "physicsSystemBindings":
            {
                value = instance.m_physicsSystemBindings;
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
            case "m_rigidBodyBindings":
            case "rigidBodyBindings":
            {
                if (value is not List<Havok.hk2014.hkpDisplayBindingDataRigidBody?> castValue) return false;
                instance.m_rigidBodyBindings = castValue;
                return true;
            }
            case "m_physicsSystemBindings":
            case "physicsSystemBindings":
            {
                if (value is not List<Havok.hk2014.hkpDisplayBindingDataPhysicsSystem?> castValue) return false;
                instance.m_physicsSystemBindings = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
