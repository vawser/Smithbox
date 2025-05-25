// Automatically Generated

namespace HKLib.hk2018;

public class hknpCompressedMeshShapeData : hkReferencedObject
{
    public hknpCompressedMeshShapeTree m_meshTree = new();

    public hkcdSimdTree m_simdTree = new();

    public hkcdStaticMeshTree.Connectivity m_connectivity = new();

    public bool m_hasSimdTree;

    public Vector3 DecompressSharedVertex(ulong vertex, Vector4 bbMin, Vector4 bbMax)
    {
        float scaleX = (bbMax.X - bbMin.X) / (float)((1 << 21) - 1);
        float scaleY = (bbMax.Y - bbMin.Y) / (float)((1 << 21) - 1);
        float scaleZ = (bbMax.Z - bbMin.Z) / (float)((1 << 22) - 1);
        float x = ((float)(vertex & 0x1FFFFF)) * scaleX + bbMin.X;
        float y = ((float)((vertex >> 21) & 0x1FFFFF)) * scaleY + bbMin.Y;
        float z = ((float)((vertex >> 42) & 0x3FFFFF)) * scaleZ + bbMin.Z;
        return new Vector3(x, y, z);
    }

    public Vector3 DecompressPackedVertex(uint vertex, Vector3 scale, Vector3 offset)
    {
        float x = ((float)(vertex & 0x7FF)) * scale.X + offset.X;
        float y = ((float)((vertex >> 11) & 0x7FF)) * scale.Y + offset.Y;
        float z = ((float)((vertex >> 22) & 0x3FF)) * scale.Z + offset.Z;
        return new Vector3(x, y, z);
    }

}

