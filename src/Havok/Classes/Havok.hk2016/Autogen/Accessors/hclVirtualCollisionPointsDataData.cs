// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclVirtualCollisionPointsDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclVirtualCollisionPointsData>
{
    public hclVirtualCollisionPointsDataData(Havok.hk2016.hclVirtualCollisionPointsData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blocks":
            case "blocks":
            {
                value = instance.m_blocks;
                return true;
            }
            case "m_numVCPoints":
            case "numVCPoints":
            {
                value = instance.m_numVCPoints;
                return true;
            }
            case "m_landscapeParticlesBlockIndex":
            case "landscapeParticlesBlockIndex":
            {
                value = instance.m_landscapeParticlesBlockIndex;
                return true;
            }
            case "m_numLandscapeVCPoints":
            case "numLandscapeVCPoints":
            {
                value = instance.m_numLandscapeVCPoints;
                return true;
            }
            case "m_edgeBarycentricsDictionary":
            case "edgeBarycentricsDictionary":
            {
                value = instance.m_edgeBarycentricsDictionary;
                return true;
            }
            case "m_edgeDictionaryEntries":
            case "edgeDictionaryEntries":
            {
                value = instance.m_edgeDictionaryEntries;
                return true;
            }
            case "m_triangleBarycentricsDictionary":
            case "triangleBarycentricsDictionary":
            {
                value = instance.m_triangleBarycentricsDictionary;
                return true;
            }
            case "m_triangleDictionaryEntries":
            case "triangleDictionaryEntries":
            {
                value = instance.m_triangleDictionaryEntries;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_edgeFans":
            case "edgeFans":
            {
                value = instance.m_edgeFans;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                value = instance.m_triangles;
                return true;
            }
            case "m_triangleFans":
            case "triangleFans":
            {
                value = instance.m_triangleFans;
                return true;
            }
            case "m_edgesLandscape":
            case "edgesLandscape":
            {
                value = instance.m_edgesLandscape;
                return true;
            }
            case "m_edgeFansLandscape":
            case "edgeFansLandscape":
            {
                value = instance.m_edgeFansLandscape;
                return true;
            }
            case "m_trianglesLandscape":
            case "trianglesLandscape":
            {
                value = instance.m_trianglesLandscape;
                return true;
            }
            case "m_triangleFansLandscape":
            case "triangleFansLandscape":
            {
                value = instance.m_triangleFansLandscape;
                return true;
            }
            case "m_edgeFanIndices":
            case "edgeFanIndices":
            {
                value = instance.m_edgeFanIndices;
                return true;
            }
            case "m_triangleFanIndices":
            case "triangleFanIndices":
            {
                value = instance.m_triangleFanIndices;
                return true;
            }
            case "m_edgeFanIndicesLandscape":
            case "edgeFanIndicesLandscape":
            {
                value = instance.m_edgeFanIndicesLandscape;
                return true;
            }
            case "m_triangleFanIndicesLandscape":
            case "triangleFanIndicesLandscape":
            {
                value = instance.m_triangleFanIndicesLandscape;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_blocks":
            case "blocks":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.Block> castValue) return false;
                instance.m_blocks = castValue;
                return true;
            }
            case "m_numVCPoints":
            case "numVCPoints":
            {
                if (value is not ushort castValue) return false;
                instance.m_numVCPoints = castValue;
                return true;
            }
            case "m_landscapeParticlesBlockIndex":
            case "landscapeParticlesBlockIndex":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_landscapeParticlesBlockIndex = castValue;
                return true;
            }
            case "m_numLandscapeVCPoints":
            case "numLandscapeVCPoints":
            {
                if (value is not ushort castValue) return false;
                instance.m_numLandscapeVCPoints = castValue;
                return true;
            }
            case "m_edgeBarycentricsDictionary":
            case "edgeBarycentricsDictionary":
            {
                if (value is not List<float> castValue) return false;
                instance.m_edgeBarycentricsDictionary = castValue;
                return true;
            }
            case "m_edgeDictionaryEntries":
            case "edgeDictionaryEntries":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.BarycentricDictionaryEntry> castValue) return false;
                instance.m_edgeDictionaryEntries = castValue;
                return true;
            }
            case "m_triangleBarycentricsDictionary":
            case "triangleBarycentricsDictionary":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.BarycentricPair> castValue) return false;
                instance.m_triangleBarycentricsDictionary = castValue;
                return true;
            }
            case "m_triangleDictionaryEntries":
            case "triangleDictionaryEntries":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.BarycentricDictionaryEntry> castValue) return false;
                instance.m_triangleDictionaryEntries = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.EdgeFanSection> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_edgeFans":
            case "edgeFans":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.EdgeFan> castValue) return false;
                instance.m_edgeFans = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.TriangleFanSection> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_triangleFans":
            case "triangleFans":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.TriangleFan> castValue) return false;
                instance.m_triangleFans = castValue;
                return true;
            }
            case "m_edgesLandscape":
            case "edgesLandscape":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.EdgeFanSection> castValue) return false;
                instance.m_edgesLandscape = castValue;
                return true;
            }
            case "m_edgeFansLandscape":
            case "edgeFansLandscape":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.EdgeFanLandscape> castValue) return false;
                instance.m_edgeFansLandscape = castValue;
                return true;
            }
            case "m_trianglesLandscape":
            case "trianglesLandscape":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.TriangleFanSection> castValue) return false;
                instance.m_trianglesLandscape = castValue;
                return true;
            }
            case "m_triangleFansLandscape":
            case "triangleFansLandscape":
            {
                if (value is not List<Havok.hk2016.hclVirtualCollisionPointsData.TriangleFanLandscape> castValue) return false;
                instance.m_triangleFansLandscape = castValue;
                return true;
            }
            case "m_edgeFanIndices":
            case "edgeFanIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_edgeFanIndices = castValue;
                return true;
            }
            case "m_triangleFanIndices":
            case "triangleFanIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleFanIndices = castValue;
                return true;
            }
            case "m_edgeFanIndicesLandscape":
            case "edgeFanIndicesLandscape":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_edgeFanIndicesLandscape = castValue;
                return true;
            }
            case "m_triangleFanIndicesLandscape":
            case "triangleFanIndicesLandscape":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleFanIndicesLandscape = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
