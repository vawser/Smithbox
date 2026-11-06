// Automatically Generated

namespace Havok.hk2014;

public class hkpMoppCode : hkReferencedObject
{
    public hkpMoppCodeCodeInfo m_info = new();

    public List<byte> m_data = new();

    public hkpMoppCode.BuildType m_buildType;


    public enum BuildType : int
    {
        BUILT_WITH_CHUNK_SUBDIVISION = 0,
        BUILT_WITHOUT_CHUNK_SUBDIVISION = 1,
        BUILD_NOT_SET = 2
    }

}
