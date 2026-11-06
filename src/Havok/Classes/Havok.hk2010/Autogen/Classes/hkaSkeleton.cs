// Automatically Generated

namespace Havok.hk2010;

public partial class hkaSkeleton : hkReferencedObject
{
    public string? m_name;

    public List<short> m_parentIndices = new();

    public List<hkaBone> m_bones = new();

    public List<hkQsTransform> m_referencePose = new();

    public List<float> m_referenceFloats = new();

    public List<string?> m_floatSlots = new();

    public List<hkaSkeleton.LocalFrameOnBone> m_localFrames = new();


    public class LocalFrameOnBone : IHavokObject
    {
        public hkLocalFrame? m_localFrame;

        public int m_boneIndex;

    }

}
