// Automatically Generated

namespace HKLib.hk2018;

public class hclSimClothData : hkReferencedObject
{
    public string? m_name;

    public hclSimClothData.OverridableSimulationInfo m_simulationInfo = new();

    public List<hclSimClothData.ParticleData> m_particleDatas = new();

    public List<ushort> m_fixedParticles = new();

    public bool m_doNormals;

    public List<uint> m_simOpIds = new();

    public List<hclSimClothPose?> m_simClothPoses = new();

    public List<hclConstraintSet?> m_staticConstraintSets = new();

    public List<hclConstraintSet?> m_antiPinchConstraintSets = new();

    public hclSimClothData.CollidableTransformMap m_collidableTransformMap = new();

    public List<hclCollidable?> m_perInstanceCollidables = new();

    public float m_maxParticleRadius;

    public List<uint> m_staticCollisionMasks = new();

    public List<hclAction?> m_actions = new();

    public float m_totalMass;

    public hclSimClothData.TransferMotionData m_transferMotionData = new();

    public bool m_transferMotionEnabled;

    public bool m_landscapeCollisionEnabled;

    public hclSimClothData.LandscapeCollisionData m_landscapeCollisionData = new();

    public uint m_numLandscapeCollidableParticles;

    public List<ushort> m_triangleIndices = new();

    public List<byte> m_triangleFlips = new();

    public bool m_pinchDetectionEnabled;

    public List<bool> m_perParticlePinchDetectionEnabledFlags = new();

    public List<hclSimClothData.CollidablePinchingData> m_collidablePinchingDatas = new();

    public ushort m_minPinchedParticleIndex;

    public ushort m_maxPinchedParticleIndex;

    public uint m_maxCollisionPairs;

    public hclVirtualCollisionPointsData m_virtualCollisionPointsData = new();


    public class CollidablePinchingData : IHavokObject
    {
        public bool m_pinchDetectionEnabled;

        public sbyte m_pinchDetectionPriority;

        public float m_pinchDetectionRadius;

    }


    public class TransferMotionData : IHavokObject
    {
        public uint m_transformSetIndex;

        public uint m_transformIndex;

        public bool m_transferTranslationMotion;

        public float m_minTranslationSpeed;

        public float m_maxTranslationSpeed;

        public float m_minTranslationBlend;

        public float m_maxTranslationBlend;

        public bool m_transferRotationMotion;

        public float m_minRotationSpeed;

        public float m_maxRotationSpeed;

        public float m_minRotationBlend;

        public float m_maxRotationBlend;

    }


    public class CollidableTransformMap : IHavokObject
    {
        public int m_transformSetIndex;

        public List<uint> m_transformIndices = new();

        public List<Matrix4x4> m_offsets = new();

    }


    public class ParticleData : IHavokObject
    {
        public float m_mass;

        public float m_invMass;

        public float m_radius;

        public float m_friction;

    }


    public class OverridableSimulationInfo : IHavokObject
    {
        public Vector4 m_gravity = new();

        public float m_globalDampingPerSecond;

    }


    public class LandscapeCollisionData : IHavokObject
    {
        public float m_landscapeRadius;

        public bool m_enableStuckParticleDetection;

        public float m_stuckParticlesStretchFactorSq;

        public bool m_pinchDetectionEnabled;

        public sbyte m_pinchDetectionPriority;

        public float m_pinchDetectionRadius;

        public float m_collisionTolerance;

    }


}

