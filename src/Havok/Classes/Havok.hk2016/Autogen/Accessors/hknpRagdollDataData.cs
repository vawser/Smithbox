// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpRagdollDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpRagdollData>
{
    public hknpRagdollDataData(Havok.hk2016.hknpRagdollData instance) : base(instance) { }

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
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                value = instance.m_microStepMultiplier;
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
            case "m_bodyTags":
            case "bodyTags":
            {
                value = instance.m_bodyTags;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_materials":
            case "materials":
            {
                if (value is not List<Havok.hk2016.hknpMaterial> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_motionProperties":
            case "motionProperties":
            {
                if (value is not List<Havok.hk2016.hknpMotionProperties> castValue) return false;
                instance.m_motionProperties = castValue;
                return true;
            }
            case "m_bodyCinfos":
            case "bodyCinfos":
            {
                if (value is not List<Havok.hk2016.hknpPhysicsSystemData.bodyCinfoWithAttachment> castValue) return false;
                instance.m_bodyCinfos = castValue;
                return true;
            }
            case "m_constraintCinfos":
            case "constraintCinfos":
            {
                if (value is not List<Havok.hk2016.hknpConstraintCinfo> castValue) return false;
                instance.m_constraintCinfos = castValue;
                return true;
            }
            case "m_referencedObjects":
            case "referencedObjects":
            {
                if (value is not List<Havok.hk2016.hkReferencedObject?> castValue) return false;
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
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                if (value is not byte castValue) return false;
                instance.m_microStepMultiplier = castValue;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is Havok.hk2016.hkaSkeleton castValue)
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
            case "m_bodyTags":
            case "bodyTags":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_bodyTags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
