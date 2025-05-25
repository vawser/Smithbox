// Automatically Generated

namespace HKLib.hk2018;

public class hkaSkeleton : hkReferencedObject
{
    public string? m_name;

    public List<short> m_parentIndices = new();

    public List<hkaBone> m_bones = new();

    public List<hkQsTransform> m_referencePose = new();

    public List<float> m_referenceFloats = new();

    public List<string?> m_floatSlots = new();

    public List<hkaSkeleton.LocalFrameOnBone> m_localFrames = new();

    public List<hkaSkeleton.Partition> m_partitions = new();


    public class Partition : IHavokObject
    {
        public string? m_name;

        public short m_startBoneIndex;

        public short m_numBones;

    }


    public class LocalFrameOnBone : IHavokObject
    {
        public hkLocalFrame? m_localFrame;

        public short m_boneIndex;

    }


}

