// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterControllerSetupData : HavokData<hkbCharacterControllerSetup> 
{
    public hkbCharacterControllerSetupData(HavokType type, hkbCharacterControllerSetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rigidBodySetup":
            case "rigidBodySetup":
            {
                if (instance.m_rigidBodySetup is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_controllerCinfo":
            case "controllerCinfo":
            {
                if (instance.m_controllerCinfo is null)
                {
                    return true;
                }
                if (instance.m_controllerCinfo is TGet castValue)
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
            case "m_rigidBodySetup":
            case "rigidBodySetup":
            {
                if (value is not hkbRigidBodySetup castValue) return false;
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
                if (value is hkReferencedObject castValue)
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
