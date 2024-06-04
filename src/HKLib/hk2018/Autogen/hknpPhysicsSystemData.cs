// Automatically Generated

namespace HKLib.hk2018;

public class hknpPhysicsSystemData : hkReferencedObject
{
    public List<hknpMaterial> m_materials = new();

    public List<hknpMotionProperties> m_motionProperties = new();

    public List<hknpPhysicsSystemData.bodyCinfoWithAttachment> m_bodyCinfos = new();

    public List<hknpConstraintCinfo> m_constraintCinfos = new();

    public string? m_name;

    public byte m_microStepMultiplier;


    public class bodyCinfoWithAttachment : hknpBodyCinfo
    {
        public int m_attachedBody;

    }


}

