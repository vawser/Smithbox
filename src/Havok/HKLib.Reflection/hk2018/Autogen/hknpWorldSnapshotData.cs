// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpWorldSnapshotData : HavokData<hknpWorldSnapshot> 
{
    public hknpWorldSnapshotData(HavokType type, hknpWorldSnapshot instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (instance.m_worldCinfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodies":
            case "bodies":
            {
                if (instance.m_bodies is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyNames":
            case "bodyNames":
            {
                if (instance.m_bodyNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motions":
            case "motions":
            {
                if (instance.m_motions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                if (instance.m_constraints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintGroupInfos":
            case "constraintGroupInfos":
            {
                if (instance.m_constraintGroupInfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleSystems":
            case "particleSystems":
            {
                if (instance.m_particleSystems is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_worldCinfo":
            case "worldCinfo":
            {
                if (value is not hknpWorldCinfo castValue) return false;
                instance.m_worldCinfo = castValue;
                return true;
            }
            case "m_bodies":
            case "bodies":
            {
                if (value is not List<hknpBody> castValue) return false;
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
                if (value is not List<hknpMotion> castValue) return false;
                instance.m_motions = castValue;
                return true;
            }
            case "m_constraints":
            case "constraints":
            {
                if (value is not List<hknpConstraintCinfo> castValue) return false;
                instance.m_constraints = castValue;
                return true;
            }
            case "m_constraintGroupInfos":
            case "constraintGroupInfos":
            {
                if (value is not List<hknpWorldSnapshot.ConstraintGroupInfo> castValue) return false;
                instance.m_constraintGroupInfos = castValue;
                return true;
            }
            case "m_particleSystems":
            case "particleSystems":
            {
                if (value is not List<hknpStorageParticleSystem> castValue) return false;
                instance.m_particleSystems = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
