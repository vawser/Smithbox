// Automatically Generated

namespace Havok.hk2018;

public class hknpParticleShapeProperties : IHavokObject
{
    public float m_particleParticleCollisionRadius;

    public float m_innerRadius;

    public float m_outerRadius;

    public float m_inverseInnerRadius;

    public float m_radiiRatio;

    public float m_radiiDifference;

    public Vector4 m_innerOuterRadius = new();

    public List<hknpParticle4Faces> m_particleFaces = new();

    public List<hknpParticleFaceVerticesWithEffMass> m_faceVertices = new();

    public float m_invInertia;

    public Vector4 m_convexRadii = new();

    public float m_allowedPenetration;

    public float m_hitPenetrationThreshold;

    public float m_hitDistanceThreshold;

    public float m_movementThreshold;

    public hknpConvexShape? m_shape;

}
