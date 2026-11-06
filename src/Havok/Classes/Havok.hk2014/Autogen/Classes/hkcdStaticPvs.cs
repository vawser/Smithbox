// Automatically Generated

namespace Havok.hk2014;

public class hkcdStaticPvs : IHavokObject
{
    public hkcdStaticTreeTreehkcdStaticTreeDynamicStorage6 m_cells = new();

    public int m_bytesPerCells;

    public int m_cellsPerBlock;

    public List<byte> m_pvs = new();

    public List<ushort> m_map = new();

    public List<hkcdStaticPvsBlockHeader> m_blocks = new();

}
