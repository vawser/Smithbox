// Automatically Generated

namespace Havok.hk2014;

public class hkbCharacterSteppedInfo : hkReferencedObject
{
    public ulong m_characterId;

    public float m_deltaTime;

    public hkQsTransform m_worldFromModel = new();

    public List<hkQsTransform> m_poseModelSpace = new();

    public List<hkQsTransform> m_rigidAttachmentTransforms = new();

}
