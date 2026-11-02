// Automatically Generated

namespace HKLib.hk2019;

public class hkImage : hkImageHeader
{
    public List<hkImage.SubImage> m_subImages = new();

    public object? m_dataPtr;

    public ulong m_dataSize;

    public ulong m_dataCapacity;

    public bool m_userExternalStorage;


    public class SubImage : IHavokObject
    {
        public ulong m_offset;

        public ulong m_size;

    }

}
