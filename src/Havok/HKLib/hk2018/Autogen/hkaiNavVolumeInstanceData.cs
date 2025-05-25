// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavVolumeInstanceData : hkReferencedObject
{
    public hkaiNavVolume? m_originalVolume;

    public List<int> m_cellMap = new();

    public List<hkaiNavVolumeInstanceData.CellInstance> m_instancedCells = new();

    public List<hkaiNavVolume.Edge> m_ownedEdges = new();

    public List<hkaiNavVolume.UserEdgeInfo> m_ownedUserEdgeInfos = new();

    public List<int> m_ownedUserEdgeData = new();

    public uint m_sectionUid;

    public int m_runtimeId;

    public int m_layerIndex;


    public class CellInstance : IHavokObject
    {
        public int m_startEdgeIndex;

        public int m_numEdges;

    }


}

