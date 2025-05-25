// Automatically Generated

namespace HKLib.hk2018;

public class hkcdPlanarSolid : hkcdPlanarEntity
{
    public hkcdPlanarSolid.NodeStorage? m_nodes;

    public hkcdPlanarEntity.PlanesCollection? m_planes;

    public uint m_rootNodeId;


    public class Node : IHavokObject
    {
        public uint m_parent;

        public uint m_left;

        public uint m_right;

        public uint m_nextFreeNodeId;

        public uint m_planeId;

        public uint m_aabbId;

        public hkcdPlanarGeometryPolygonCollection.Material m_material = new();

        public uint m_data;

        public ushort m_type;

        public ushort m_flags;

    }


    public class NodeStorage : hkReferencedObject
    {
        public List<hkcdPlanarSolid.Node> m_storage = new();

        public List<hkAabb> m_aabbs = new();

        public uint m_firstFreeNodeId;

    }


}

