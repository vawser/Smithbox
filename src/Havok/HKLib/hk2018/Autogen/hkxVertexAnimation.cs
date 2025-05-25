// Automatically Generated

namespace HKLib.hk2018;

public class hkxVertexAnimation : hkReferencedObject
{
    public float m_time;

    public hkxVertexBuffer m_vertData = new();

    public List<int> m_vertexIndexMap = new();

    public List<hkxVertexAnimation.UsageMap> m_componentMap = new();


    public class UsageMap : IHavokObject
    {
        public hkxVertexDescription.DataUsage m_use;

        public byte m_useIndexOrig;

        public byte m_useIndexLocal;

    }


}

