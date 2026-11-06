// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpDisplayBindingDataPhysicsSystemData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpDisplayBindingDataPhysicsSystem>
{
    public hkpDisplayBindingDataPhysicsSystemData(Havok.hk2014.hkpDisplayBindingDataPhysicsSystem instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bindings":
            case "bindings":
            {
                value = instance.m_bindings;
                return true;
            }
            case "m_system":
            case "system":
            {
                value = instance.m_system;
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
            case "m_bindings":
            case "bindings":
            {
                if (value is not List<Havok.hk2014.hkpDisplayBindingDataRigidBody?> castValue) return false;
                instance.m_bindings = castValue;
                return true;
            }
            case "m_system":
            case "system":
            {
                if (value is null)
                {
                    instance.m_system = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpPhysicsSystem castValue)
                {
                    instance.m_system = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
