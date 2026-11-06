// Automatically Generated

namespace Havok.hk2019;

public class hkxMeshSection : hkReferencedObject
{
    public hkxVertexBuffer? m_vertexBuffer;

    public List<hkxIndexBuffer?> m_indexBuffers = new();

    public hkxMaterial? m_material;

    public List<hkReferencedObject?> m_userChannels = new();

    public List<hkxVertexAnimation?> m_vertexAnimations = new();

    public List<float> m_linearKeyFrameHints = new();

    public List<hkMeshBoneIndexMapping> m_boneMatrixMap = new();

    public List<hkxMeshSection.InstanceInfo> m_instances = new();

    public Vector4 m_originalBoundingBoxMin = new();

    public Vector4 m_originalBoundingBoxMax = new();


    public class InstanceInfo : IHavokObject
    {
        public Matrix4x4 m_localTransform = new();

        public uint m_vertexBase;

        public uint m_verticesCount;

        public uint m_indexOffset;

        public uint m_indicesCount;

    }

}
