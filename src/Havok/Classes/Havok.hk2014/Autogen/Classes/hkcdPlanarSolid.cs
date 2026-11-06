// Automatically Generated

namespace Havok.hk2014;

public class hkcdPlanarSolid : hkcdPlanarEntity
{
    public hkcdPlanarSolid.NodeStorage? m_nodes;

    public hkcdPlanarGeometryPlanesCollection? m_planes;

    public uint m_rootNodeId;


    public class NodeStorage : hkReferencedObject
    {
        public List<hkcdPlanarSolid.Node> m_storage = new();

        public uint m_firstFreeNodeId;

    }

    public class Node : IHavokObject
    {
        public uint m_parent;

        public uint m_left;

        public uint m_right;

        public uint m_nextFreeNodeId;

        public uint m_planeId;

        public uint m_data;

        public uint m_typeAndFlags;

    }

}
