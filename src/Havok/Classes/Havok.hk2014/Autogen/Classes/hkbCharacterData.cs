// Automatically Generated

namespace Havok.hk2014;

public class hkbCharacterData : hkReferencedObject
{
    public hkbCharacterControllerSetup m_characterControllerSetup = new();

    public Vector4 m_modelUpMS = new();

    public Vector4 m_modelForwardMS = new();

    public Vector4 m_modelRightMS = new();

    public List<hkbVariableInfo> m_characterPropertyInfos = new();

    public List<int> m_numBonesPerLod = new();

    public hkbVariableValueSet? m_characterPropertyValues;

    public hkbFootIkDriverInfo? m_footIkDriverInfo;

    public hkbHandIkDriverInfo? m_handIkDriverInfo;

    public hkReferencedObject? m_aiControlDriverInfo;

    public hkbCharacterStringData? m_stringData;

    public hkbMirroredSkeletonInfo? m_mirroredSkeletonInfo;

    public List<short> m_boneAttachmentBoneIndices = new();

    public List<Matrix4x4> m_boneAttachmentTransforms = new();

    public float m_scale;

}
