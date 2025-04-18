// Automatically Generated

namespace HKLib.hk2018.hkcdStaticMeshTree;

public class Connectivity : IHavokObject
{
    public List<hkcdStaticMeshTree.Connectivity.SectionHeader> m_headers = new();

    public List<byte> m_localLinks = new();

    public List<hkHandle<uint>> m_globalLinks = new();


    public class SectionHeader : IHavokObject
    {
        public uint m_baseLocal;

        public uint m_baseGlobal;

    }


}

