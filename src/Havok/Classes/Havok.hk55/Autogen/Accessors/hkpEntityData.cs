// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpEntityData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpEntity>
{
    public hkpEntityData(Havok.hk55.hkpEntity instance) : base(instance) { }

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
            case "m_storageIndex":
            case "storageIndex":
            {
                value = instance.m_storageIndex;
                return true;
            }
            case "m_processContactCallbackDelay":
            case "processContactCallbackDelay":
            {
                value = instance.m_processContactCallbackDelay;
                return true;
            }
            case "m_autoRemoveLevel":
            case "autoRemoveLevel":
            {
                value = instance.m_autoRemoveLevel;
                return true;
            }
            case "m_numUserDatasInContactPointProperties":
            case "numUserDatasInContactPointProperties":
            {
                value = instance.m_numUserDatasInContactPointProperties;
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
                if (value is not Havok.hk55.hkpLinkedCollidable castValue) return false;
                instance.m_collidable = castValue;
                return true;
            }
            case "m_multiThreadCheck":
            case "multiThreadCheck":
            {
                if (value is not Havok.hk55.hkMultiThreadCheck castValue) return false;
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
                if (value is not List<Havok.hk55.hkpProperty> castValue) return false;
                instance.m_properties = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is not Havok.hk55.hkpMaterial castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            case "m_storageIndex":
            case "storageIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_storageIndex = castValue;
                return true;
            }
            case "m_processContactCallbackDelay":
            case "processContactCallbackDelay":
            {
                if (value is not ushort castValue) return false;
                instance.m_processContactCallbackDelay = castValue;
                return true;
            }
            case "m_autoRemoveLevel":
            case "autoRemoveLevel":
            {
                if (value is not sbyte castValue) return false;
                instance.m_autoRemoveLevel = castValue;
                return true;
            }
            case "m_numUserDatasInContactPointProperties":
            case "numUserDatasInContactPointProperties":
            {
                if (value is not byte castValue) return false;
                instance.m_numUserDatasInContactPointProperties = castValue;
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
                if (value is not Havok.hk55.hkpEntitySpuCollisionCallback castValue) return false;
                instance.m_spuCollisionCallback = castValue;
                return true;
            }
            case "m_motion":
            case "motion":
            {
                if (value is not Havok.hk55.hkpMaxSizeMotion castValue) return false;
                instance.m_motion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
