// Automatically Generated

namespace HKLib.hk2018;

public class hclTransformSetUsage : IHavokObject
{
    public readonly byte[] m_perComponentFlags = new byte[2];

    public List<hclTransformSetUsage.TransformTracker> m_perComponentTransformTrackers = new();


    public class TransformTracker : IHavokObject
    {
        public hkBitField m_read = new();

        public hkBitField m_readBeforeWrite = new();

        public hkBitField m_written = new();

    }


}

