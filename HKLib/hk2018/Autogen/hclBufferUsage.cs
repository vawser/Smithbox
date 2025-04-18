// Automatically Generated

namespace HKLib.hk2018;

public class hclBufferUsage : IHavokObject
{
    public readonly byte[] m_perComponentFlags = new byte[4];

    public bool m_trianglesRead;


    public enum Component : int
    {
        COMPONENT_POSITION = 0,
        COMPONENT_NORMAL = 1,
        COMPONENT_TANGENT = 2,
        COMPONENT_BITANGENT = 3
    }

}

