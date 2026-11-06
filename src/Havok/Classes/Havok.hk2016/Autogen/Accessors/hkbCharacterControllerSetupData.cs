// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbCharacterControllerSetupData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbCharacterControllerSetup>
{
    public hkbCharacterControllerSetupData(Havok.hk2016.hkbCharacterControllerSetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rigidBodySetup":
            case "rigidBodySetup":
            {
                value = instance.m_rigidBodySetup;
                return true;
            }
            case "m_controllerCinfo":
            case "controllerCinfo":
            {
                value = instance.m_controllerCinfo;
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
            case "m_rigidBodySetup":
            case "rigidBodySetup":
            {
                if (value is not Havok.hk2016.hkbRigidBodySetup castValue) return false;
                instance.m_rigidBodySetup = castValue;
                return true;
            }
            case "m_controllerCinfo":
            case "controllerCinfo":
            {
                if (value is null)
                {
                    instance.m_controllerCinfo = default;
                    return true;
                }
                if (value is Havok.hk2016.hkReferencedObject castValue)
                {
                    instance.m_controllerCinfo = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
