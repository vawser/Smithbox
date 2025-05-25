// Automatically Generated

namespace HKLib.hk2018;

public class hclSimClothSetupObject : hkReferencedObject
{
    public string? m_name;

    public hclSimulationSetupMesh? m_simulationMesh;

    public hclTransformSetSetupObject? m_collidableTransformSet;

    public Vector4 m_gravity = new();

    public float m_globalDampingPerSecond;

    public bool m_doNormals;

    public bool m_specifyDensity;

    public hclVertexFloatInput m_vertexDensity = new();

    public bool m_rescaleMass;

    public float m_totalMass;

    public hclVertexFloatInput m_particleMass = new();

    public hclVertexFloatInput m_particleRadius = new();

    public hclVertexFloatInput m_particleFriction = new();

    public hclVertexSelectionInput m_fixedParticles = new();

    public bool m_enablePinchDetection;

    public hclVertexSelectionInput m_pinchDetectionEnabledParticles = new();

    public float m_toAnimPeriod;

    public float m_toSimPeriod;

    public bool m_drivePinchedParticlesToReferenceMesh;

    public hclBufferSetupObject? m_pinchReferenceBufferSetup;

    public float m_collisionTolerance;

    public hclVertexSelectionInput m_landscapeCollisionParticleSelection = new();

    public float m_landscapeCollisionParticleRadius;

    public bool m_enableStuckParticleDetection;

    public float m_stuckParticlesStretchFactor;

    public bool m_enableLandscapePinchDetection;

    public sbyte m_landscapePinchDetectionPriority;

    public float m_landscapePinchDetectionRadius;

    public bool m_enableTransferMotion;

    public hclSimClothSetupObject.TransferMotionSetupData m_transferMotionSetupData = new();

    public hclVertexSelectionInput m_virtualCollisionPoints = new();

    public hclVertexFloatInput m_virtualCollisionPointDensities = new();

    public bool m_virtualCollisionPointUseAllCollidables;

    public List<string?> m_virtualCollisionPointCollidables = new();

    public hclVertexSelectionInput m_landscapeVirtualCollisionPoints = new();

    public hclVertexFloatInput m_landscapeVirtualCollisionPointDensities = new();

    public List<hclConstraintSetSetupObject?> m_constraintSetSetups = new();

    public List<hclSimClothSetupObject.PerInstanceCollidable> m_perInstanceCollidables = new();


    public class PerInstanceCollidable : IHavokObject
    {
        public hclCollidable? m_collidable;

        public hclVertexSelectionInput m_collidingParticles = new();

        public string? m_drivingBoneName;

        public bool m_pinchDetectionEnabled;

        public sbyte m_pinchDetectionPriority;

        public float m_pinchDetectionRadius;

        public bool m_vcpCollisionEnabled;

    }


    public class TransferMotionSetupData : IHavokObject
    {
        public hclTransformSetSetupObject? m_transferMotionTransformSetSetup;

        public string? m_transferMotionTransformName;

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


}

