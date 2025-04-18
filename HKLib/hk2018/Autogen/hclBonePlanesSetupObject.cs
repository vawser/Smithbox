// Automatically Generated

namespace HKLib.hk2018;

public class hclBonePlanesSetupObject : hclConstraintSetSetupObject
{
    public string? m_name;

    public hclSimulationSetupMesh? m_simulationMesh;

    public hclTransformSetSetupObject? m_transformSetSetup;

    public List<hclBonePlanesSetupObject.PerParticlePlane> m_perParticlePlanes = new();

    public List<hclBonePlanesSetupObject.GlobalPlane> m_globalPlanes = new();

    public List<hclBonePlanesSetupObject.PerParticleAngle> m_perParticleAngle = new();

    public bool m_angleSpecifiedInDegrees;


    public class PerParticleAngle : IHavokObject
    {
        public string? m_transformName;

        public hclVertexSelectionInput m_particlesMaxAngle = new();

        public hclVertexSelectionInput m_particlesMinAngle = new();

        public Vector4 m_originBoneSpace = new();

        public Vector4 m_axisBoneSpace = new();

        public hclVertexFloatInput m_minAngle = new();

        public hclVertexFloatInput m_maxAngle = new();

        public hclVertexFloatInput m_stiffness = new();

    }


    public class GlobalPlane : IHavokObject
    {
        public string? m_transformName;

        public hclVertexSelectionInput m_particles = new();

        public Vector4 m_planeEquationBoneSpace = new();

        public hclVertexFloatInput m_allowedPenetration = new();

        public hclVertexFloatInput m_stiffness = new();

    }


    public class PerParticlePlane : IHavokObject
    {
        public string? m_transformName;

        public hclVertexSelectionInput m_particles = new();

        public Vector4 m_directionBoneSpace = new();

        public hclVertexFloatInput m_allowedDistance = new();

        public hclVertexFloatInput m_stiffness = new();

    }


}

