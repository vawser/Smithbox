// Automatically Generated

namespace HKLib.hk2018;

public class hclClothState : hkReferencedObject
{
    public string? m_name;

    public List<uint> m_operators = new();

    public List<hclClothState.BufferAccess> m_usedBuffers = new();

    public List<hclClothState.TransformSetAccess> m_usedTransformSets = new();

    public List<uint> m_usedSimCloths = new();

    public hclStateDependencyGraph? m_dependencyGraph;


    public class TransformSetAccess : IHavokObject
    {
        public uint m_transformSetIndex;

        public hclTransformSetUsage m_transformSetUsage = new();

    }


    public class BufferAccess : IHavokObject
    {
        public uint m_bufferIndex;

        public hclBufferUsage m_bufferUsage = new();

        public uint m_shadowBufferIndex;

    }


}

