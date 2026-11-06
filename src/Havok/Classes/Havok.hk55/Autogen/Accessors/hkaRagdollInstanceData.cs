// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaRagdollInstanceData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaRagdollInstance>
{
    public hkaRagdollInstanceData(Havok.hk55.hkaRagdollInstance instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rigidBodies":
            case "rigidBodies":
            {
                value = instance.m_rigidBodies;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                value = instance.m_constraints;
                return true;
            }
            case "m_boneToRigidBodyMap":
            case "boneToRigidBodyMap":
            {
                value = instance.m_boneToRigidBodyMap;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                value = instance.m_skeleton;
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
            case "m_rigidBodies":
            case "rigidBodies":
            {
                if (value is not List<Havok.hk55.hkpRigidBody?> castValue) return false;
                instance.m_rigidBodies = castValue;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                if (value is not List<Havok.hk55.hkpConstraintInstance?> castValue) return false;
                instance.m_constraints = castValue;
                return true;
            }
            case "m_boneToRigidBodyMap":
            case "boneToRigidBodyMap":
            {
                if (value is not List<int> castValue) return false;
                instance.m_boneToRigidBodyMap = castValue;
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
                if (value is Havok.hk55.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
