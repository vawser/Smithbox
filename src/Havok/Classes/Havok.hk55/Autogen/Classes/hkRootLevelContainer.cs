// Automatically Generated

using Havok.Shared;

namespace Havok.hk55;

public class hkRootLevelContainer : IHavokObject, IRootContainer
{
    public List<hkRootLevelContainer.NamedVariant> m_namedVariants = new();


    public class NamedVariant : IHavokObject
    {
        public string? m_name;

        public string? m_className;

        public hkVariant m_variant = new();

    }

}
