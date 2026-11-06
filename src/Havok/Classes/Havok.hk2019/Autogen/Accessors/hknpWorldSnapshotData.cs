// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpWorldSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpWorldSnapshot>
{
    public hknpWorldSnapshotData(Havok.hk2019.hknpWorldSnapshot instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_worldCinfo":
            case "worldCinfo":
            {
                value = instance.m_worldCinfo;
                return true;
            }
            case "m_bodies":
            case "bodies":
            {
                value = instance.m_bodies;
                return true;
            }
            case "m_bodyNames":
            case "bodyNames":
            {
                value = instance.m_bodyNames;
                return true;
            }
            case "m_motions":
            case "motions":
            {
                value = instance.m_motions;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                value = instance.m_constraints;
                return true;
            }
            case "m_stepCount":
            case "stepCount":
            {
                value = instance.m_stepCount;
                return true;
            }
            case "m_constraintGroupInfos":
            case "constraintGroupInfos":
            {
                value = instance.m_constraintGroupInfos;
                return true;
            }
            case "m_particleSystems":
            case "particleSystems":
            {
                value = instance.m_particleSystems;
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
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (value is not Havok.hk2019.hknpWorldCinfo castValue) return false;
                instance.m_worldCinfo = castValue;
                return true;
            }
            case "m_bodies":
            case "bodies":
            {
                if (value is not List<Havok.hk2019.hknpBody> castValue) return false;
                instance.m_bodies = castValue;
                return true;
            }
            case "m_bodyNames":
            case "bodyNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_bodyNames = castValue;
                return true;
            }
            case "m_motions":
            case "motions":
            {
                if (value is not List<Havok.hk2019.hknpMotion> castValue) return false;
                instance.m_motions = castValue;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                if (value is not List<Havok.hk2019.hknpConstraintCinfo> castValue) return false;
                instance.m_constraints = castValue;
                return true;
            }
            case "m_stepCount":
            case "stepCount":
            {
                if (value is not uint castValue) return false;
                instance.m_stepCount = castValue;
                return true;
            }
            case "m_constraintGroupInfos":
            case "constraintGroupInfos":
            {
                if (value is not List<Havok.hk2019.hknpWorldSnapshot.ConstraintGroupInfo> castValue) return false;
                instance.m_constraintGroupInfos = castValue;
                return true;
            }
            case "m_particleSystems":
            case "particleSystems":
            {
                if (value is not List<Havok.hk2019.hknpStorageParticleSystem> castValue) return false;
                instance.m_particleSystems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
