// Automatically Generated

namespace HKLib.hk2018;

public class hkSkinnedMeshShape : hkReferencedObject
{

    public class Part : IHavokObject
    {
        public int m_startVertex;

        public int m_numVertices;

        public int m_startIndex;

        public int m_numIndices;

        public ushort m_boneSetId;

        public ushort m_meshSectionIndex;

        public Vector4 m_boundingSphere = new();

    }


    public class BoneSection : IHavokObject
    {
        public hkMeshShape? m_meshBuffer;

        public ushort m_startBoneSetId;

        public short m_numBoneSets;

    }


    public class BoneSet : IHavokObject
    {
        public ushort m_boneBufferOffset;

        public ushort m_numBones;

    }


}

