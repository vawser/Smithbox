// Automatically Generated

namespace HKLib.hk2018;

public class hknpCompoundShape : hknpCompositeShape
{
    public hkFreeListArrayHknpShapeInstance m_instances = new();

    public List<hknpCompoundShape.VelocityInfo> m_instanceVelocities = new();

    public hkAabb m_aabb = new();

    public float m_boundingRadius;

    public bool m_isMutable;

    public int m_estimatedNumShapeKeys;

    public hknpCompoundShapeData m_boundingVolumeData = new();


    public class VelocityInfo : IHavokObject
    {
        public Vector4 m_linearVelocity = new();

        public Vector4 m_angularVelocity = new();

    }


}

