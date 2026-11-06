// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpRagdollDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpRagdollData>
{
    public hknpRagdollDataData(Havok.hk2014.hknpRagdollData instance) : base(instance) { }

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
            case "m_skeleton":
            case "skeleton":
            {
                value = instance.m_skeleton;
                return true;
            }
            case "m_boneToBodyMap":
            case "boneToBodyMap":
            {
                value = instance.m_boneToBodyMap;
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
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneToBodyMap":
            case "boneToBodyMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_boneToBodyMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
