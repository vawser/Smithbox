// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpWorldSnapshotData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpWorldSnapshot>
{
    public hknpWorldSnapshotData(Havok.hk2018.hknpWorldSnapshot instance) : base(instance) { }

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (value is not Havok.hk2018.hknpWorldCinfo castValue) return false;
                instance.m_worldCinfo = castValue;
                return true;
            }
            case "m_bodies":
            case "bodies":
            {
                if (value is not List<Havok.hk2018.hknpBody> castValue) return false;
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
                if (value is not List<Havok.hk2018.hknpMotion> castValue) return false;
                instance.m_motions = castValue;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                if (value is not List<Havok.hk2018.hknpConstraintCinfo> castValue) return false;
                instance.m_constraints = castValue;
                return true;
            }
            case "m_constraintGroupInfos":
            case "constraintGroupInfos":
            {
                if (value is not List<Havok.hk2018.hknpWorldSnapshot.ConstraintGroupInfo> castValue) return false;
                instance.m_constraintGroupInfos = castValue;
                return true;
            }
            case "m_particleSystems":
            case "particleSystems":
            {
                if (value is not List<Havok.hk2018.hknpStorageParticleSystem> castValue) return false;
                instance.m_particleSystems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
