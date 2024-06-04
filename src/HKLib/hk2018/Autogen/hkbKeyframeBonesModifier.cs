// Automatically Generated

namespace HKLib.hk2018;

public class hkbKeyframeBonesModifier : hkbModifier, hkbVerifiable
{
    public List<hkbKeyframeBonesModifier.KeyframeInfo> m_keyframeInfo = new();

    public hkbBoneIndexArray? m_keyframedBonesList;


    public class KeyframeInfo : IHavokObject
    {
        public Vector4 m_keyframedPosition = new();

        public Quaternion m_keyframedRotation = new();

        public short m_boneIndex;

        public bool m_isValid;

    }


}

