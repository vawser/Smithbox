// Automatically Generated

namespace HKLib.hk2018;

public class hkMemoryResourceHandle : hkResourceHandle
{
    public hkReferencedObject? m_variant;

    public string? m_name;

    public List<hkMemoryResourceHandle.ExternalLink> m_references = new();


    public class ExternalLink : IHavokObject
    {
        public string? m_memberName;

        public string? m_externalId;

    }


}

