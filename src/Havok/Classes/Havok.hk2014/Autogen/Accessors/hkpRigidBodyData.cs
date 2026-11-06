// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpRigidBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpRigidBody>
{
    public hkpRigidBodyData(Havok.hk2014.hkpRigidBody instance) : base(instance) { }

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
            case "m_collidable":
            case "collidable":
            {
                value = instance.m_collidable;
                return true;
            }
            case "m_multiThreadCheck":
            case "multiThreadCheck":
            {
                value = instance.m_multiThreadCheck;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_properties":
            case "properties":
            {
                value = instance.m_properties;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_damageMultiplier":
            case "damageMultiplier":
            {
                value = instance.m_damageMultiplier;
                return true;
            }
            case "m_storageIndex":
            case "storageIndex":
            {
                value = instance.m_storageIndex;
                return true;
            }
            case "m_contactPointCallbackDelay":
            case "contactPointCallbackDelay":
            {
                value = instance.m_contactPointCallbackDelay;
                return true;
            }
            case "m_autoRemoveLevel":
            case "autoRemoveLevel":
            {
                value = instance.m_autoRemoveLevel;
                return true;
            }
            case "m_numShapeKeysInContactPointProperties":
            case "numShapeKeysInContactPointProperties":
            {
                value = instance.m_numShapeKeysInContactPointProperties;
                return true;
            }
            case "m_responseModifierFlags":
            case "responseModifierFlags":
            {
                value = instance.m_responseModifierFlags;
                return true;
            }
            case "m_uid":
            case "uid":
            {
                value = instance.m_uid;
                return true;
            }
            case "m_spuCollisionCallback":
            case "spuCollisionCallback":
            {
                value = instance.m_spuCollisionCallback;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                value = instance.m_motion;
                return true;
            }
            case "m_localFrame":
            case "localFrame":
            {
                value = instance.m_localFrame;
                return true;
            }
            case "m_npData":
            case "npData":
            {
                value = instance.m_npData;
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
            case "m_collidable":
            case "collidable":
            {
                if (value is not Havok.hk2014.hkpLinkedCollidable castValue) return false;
                instance.m_collidable = castValue;
                return true;
            }
            case "m_multiThreadCheck":
            case "multiThreadCheck":
            {
                if (value is not Havok.hk2014.hkMultiThreadCheck castValue) return false;
                instance.m_multiThreadCheck = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_properties":
            case "properties":
            {
                if (value is not List<Havok.hk2014.hkSimpleProperty> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is not Havok.hk2014.hkpMaterial castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            case "m_damageMultiplier":
            case "damageMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_damageMultiplier = castValue;
                return true;
            }
            case "m_storageIndex":
            case "storageIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_storageIndex = castValue;
                return true;
            }
            case "m_contactPointCallbackDelay":
            case "contactPointCallbackDelay":
            {
                if (value is not ushort castValue) return false;
                instance.m_contactPointCallbackDelay = castValue;
                return true;
            }
            case "m_autoRemoveLevel":
            case "autoRemoveLevel":
            {
                if (value is not sbyte castValue) return false;
                instance.m_autoRemoveLevel = castValue;
                return true;
            }
            case "m_numShapeKeysInContactPointProperties":
            case "numShapeKeysInContactPointProperties":
            {
                if (value is not byte castValue) return false;
                instance.m_numShapeKeysInContactPointProperties = castValue;
                return true;
            }
            case "m_responseModifierFlags":
            case "responseModifierFlags":
            {
                if (value is not byte castValue) return false;
                instance.m_responseModifierFlags = castValue;
                return true;
            }
            case "m_uid":
            case "uid":
            {
                if (value is not uint castValue) return false;
                instance.m_uid = castValue;
                return true;
            }
            case "m_spuCollisionCallback":
            case "spuCollisionCallback":
            {
                if (value is not Havok.hk2014.hkpEntitySpuCollisionCallback castValue) return false;
                instance.m_spuCollisionCallback = castValue;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                if (value is not Havok.hk2014.hkpMaxSizeMotion castValue) return false;
                instance.m_motion = castValue;
                return true;
            }
            case "m_localFrame":
            case "localFrame":
            {
                if (value is null)
                {
                    instance.m_localFrame = default;
                    return true;
                }
                if (value is Havok.hk2014.hkLocalFrame castValue)
                {
                    instance.m_localFrame = castValue;
                    return true;
                }
                return false;
            }
            case "m_npData":
            case "npData":
            {
                if (value is not uint castValue) return false;
                instance.m_npData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
