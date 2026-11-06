// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpPhysicsSystemDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpPhysicsSystemData>
{
    public hknpPhysicsSystemDataData(Havok.hk2014.hknpPhysicsSystemData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_materials":
            case "materials":
            {
                value = instance.m_materials;
                return true;
            }
            case "m_motionProperties":
            case "motionProperties":
            {
                value = instance.m_motionProperties;
                return true;
            }
            case "m_motionCinfos":
            case "motionCinfos":
            {
                value = instance.m_motionCinfos;
                return true;
            }
            case "m_bodyCinfos":
            case "bodyCinfos":
            {
                value = instance.m_bodyCinfos;
                return true;
            }
            case "m_constraintCinfos":
            case "constraintCinfos":
            {
                value = instance.m_constraintCinfos;
                return true;
            }
            case "m_referencedObjects":
            case "referencedObjects":
            {
                value = instance.m_referencedObjects;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_materials":
            case "materials":
            {
                if (value is not List<Havok.hk2014.hknpMaterial> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_motionProperties":
            case "motionProperties":
            {
                if (value is not List<Havok.hk2014.hknpMotionProperties> castValue) return false;
                instance.m_motionProperties = castValue;
                return true;
            }
            case "m_motionCinfos":
            case "motionCinfos":
            {
                if (value is not List<Havok.hk2014.hknpMotionCinfo> castValue) return false;
                instance.m_motionCinfos = castValue;
                return true;
            }
            case "m_bodyCinfos":
            case "bodyCinfos":
            {
                if (value is not List<Havok.hk2014.hknpBodyCinfo> castValue) return false;
                instance.m_bodyCinfos = castValue;
                return true;
            }
            case "m_constraintCinfos":
            case "constraintCinfos":
            {
                if (value is not List<Havok.hk2014.hknpConstraintCinfo> castValue) return false;
                instance.m_constraintCinfos = castValue;
                return true;
            }
            case "m_referencedObjects":
            case "referencedObjects":
            {
                if (value is not List<Havok.hk2014.hkReferencedObject?> castValue) return false;
                instance.m_referencedObjects = castValue;
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
            default:
            return false;
        }
    }

}
