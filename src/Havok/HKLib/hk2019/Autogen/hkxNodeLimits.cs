// Automatically Generated

namespace HKLib.hk2019;

public class hkxNodeLimits : hkReferencedObject
{
    public hkxNodeLimits.Limit_2 m_translationLimit = new();

    public hkxNodeLimits.Limit_2 m_rotationLimit = new();

    public hkxNodeLimits.Limit_2 m_scaleLimit = new();


    public class Limit : IHavokObject
    {

        public enum Element : int
        {
            MinX = 0,
            MinY = 1,
            MinZ = 2,
            MaxX = 3,
            MaxY = 4,
            MaxZ = 5,
            NumElements = 6
        }

    }

    public class Limit_2 : IHavokObject
    {
        public readonly float[] m_values = new float[6];

        public byte m_active;

    }

}
