// Automatically Generated

namespace HKLib.hk2018;

public class hknpWorldSnapshot : hkReferencedObject
{
    public hknpWorldCinfo m_worldCinfo = new();

    public List<hknpBody> m_bodies = new();

    public List<string?> m_bodyNames = new();

    public List<hknpMotion> m_motions = new();

    public List<hknpConstraintCinfo> m_constraints = new();

    public List<hknpWorldSnapshot.ConstraintGroupInfo> m_constraintGroupInfos = new();

    public List<hknpStorageParticleSystem> m_particleSystems = new();


    public class ConstraintGroupInfo : IHavokObject
    {
        public hknpConstraintGroupId m_id = new();

        public byte m_multiplier;

    }


}

