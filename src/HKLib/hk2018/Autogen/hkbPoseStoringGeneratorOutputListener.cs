// Automatically Generated

namespace HKLib.hk2018;

public class hkbPoseStoringGeneratorOutputListener : hkbGeneratorOutputListener
{
    public List<hkbPoseStoringGeneratorOutputListener.StoredPose?> m_storedPoses = new();

    public bool m_dirty;


    public class StoredPose : hkReferencedObject
    {
        public hkbNode? m_node;

        public List<hkQsTransform> m_pose = new();

        public hkQsTransform m_worldFromModel = new();

        public bool m_isPoseValid;

    }


}

