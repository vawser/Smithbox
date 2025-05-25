// Automatically Generated

namespace HKLib.hk2018;

public class hknpParticleShapeProperties : IHavokObject
{
    public Vector128<float> m_particleParticleCollisionRadius;

    public Vector128<float> m_innerRadius;

    public Vector128<float> m_outerRadius;

    public Vector128<float> m_inverseInnerRadius;

    public Vector128<float> m_radiiRatio;

    public Vector128<float> m_radiiDifference;

    public Vector4 m_innerOuterRadius = new();

    public List<hknpParticle4Faces> m_particleFaces = new();

    public List<hknpParticleFaceVerticesWithEffMass> m_faceVertices = new();

    public Vector128<float> m_invInertia;

    public Vector4 m_convexRadii = new();

    public Vector128<float> m_allowedPenetration;

    public Vector128<float> m_hitPenetrationThreshold;

    public Vector128<float> m_hitDistanceThreshold;

    public Vector128<float> m_movementThreshold;

    public hknpConvexShape? m_shape;

}

