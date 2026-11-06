// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpDisplayBindingDataRigidBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpDisplayBindingDataRigidBody>
{
    public hkpDisplayBindingDataRigidBodyData(Havok.hk2014.hkpDisplayBindingDataRigidBody instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rigidBody":
            case "rigidBody":
            {
                value = instance.m_rigidBody;
                return true;
            }
            case "m_displayObjectPtr":
            case "displayObjectPtr":
            {
                value = instance.m_displayObjectPtr;
                return true;
            }
            case "m_rigidBodyFromDisplayObjectTransform":
            case "rigidBodyFromDisplayObjectTransform":
            {
                value = instance.m_rigidBodyFromDisplayObjectTransform;
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
            case "m_rigidBody":
            case "rigidBody":
            {
                if (value is null)
                {
                    instance.m_rigidBody = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpRigidBody castValue)
                {
                    instance.m_rigidBody = castValue;
                    return true;
                }
                return false;
            }
            case "m_displayObjectPtr":
            case "displayObjectPtr":
            {
                if (value is null)
                {
                    instance.m_displayObjectPtr = default;
                    return true;
                }
                if (value is Havok.hk2014.hkReferencedObject castValue)
                {
                    instance.m_displayObjectPtr = castValue;
                    return true;
                }
                return false;
            }
            case "m_rigidBodyFromDisplayObjectTransform":
            case "rigidBodyFromDisplayObjectTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_rigidBodyFromDisplayObjectTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
