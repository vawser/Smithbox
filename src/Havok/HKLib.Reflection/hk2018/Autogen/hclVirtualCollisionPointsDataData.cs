// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVirtualCollisionPointsDataData : HavokData<hclVirtualCollisionPointsData> 
{
    public hclVirtualCollisionPointsDataData(HavokType type, hclVirtualCollisionPointsData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_blocks":
            case "blocks":
            {
                if (instance.m_blocks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numVCPoints":
            case "numVCPoints":
            {
                if (instance.m_numVCPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_landscapeParticlesBlockIndex":
            case "landscapeParticlesBlockIndex":
            {
                if (instance.m_landscapeParticlesBlockIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numLandscapeVCPoints":
            case "numLandscapeVCPoints":
            {
                if (instance.m_numLandscapeVCPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeBarycentricsDictionary":
            case "edgeBarycentricsDictionary":
            {
                if (instance.m_edgeBarycentricsDictionary is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeDictionaryEntries":
            case "edgeDictionaryEntries":
            {
                if (instance.m_edgeDictionaryEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleBarycentricsDictionary":
            case "triangleBarycentricsDictionary":
            {
                if (instance.m_triangleBarycentricsDictionary is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleDictionaryEntries":
            case "triangleDictionaryEntries":
            {
                if (instance.m_triangleDictionaryEntries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeFans":
            case "edgeFans":
            {
                if (instance.m_edgeFans is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (instance.m_triangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFans":
            case "triangleFans":
            {
                if (instance.m_triangleFans is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgesLandscape":
            case "edgesLandscape":
            {
                if (instance.m_edgesLandscape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeFansLandscape":
            case "edgeFansLandscape":
            {
                if (instance.m_edgeFansLandscape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_trianglesLandscape":
            case "trianglesLandscape":
            {
                if (instance.m_trianglesLandscape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFansLandscape":
            case "triangleFansLandscape":
            {
                if (instance.m_triangleFansLandscape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeFanIndices":
            case "edgeFanIndices":
            {
                if (instance.m_edgeFanIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFanIndices":
            case "triangleFanIndices":
            {
                if (instance.m_triangleFanIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeFanIndicesLandscape":
            case "edgeFanIndicesLandscape":
            {
                if (instance.m_edgeFanIndicesLandscape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleFanIndicesLandscape":
            case "triangleFanIndicesLandscape":
            {
                if (instance.m_triangleFanIndicesLandscape is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_blocks":
            case "blocks":
            {
                if (value is not List<hclVirtualCollisionPointsData.Block> castValue) return false;
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
                if (value is not List<hclVirtualCollisionPointsData.BarycentricDictionaryEntry> castValue) return false;
                instance.m_edgeDictionaryEntries = castValue;
                return true;
            }
            case "m_triangleBarycentricsDictionary":
            case "triangleBarycentricsDictionary":
            {
                if (value is not List<hclVirtualCollisionPointsData.BarycentricPair> castValue) return false;
                instance.m_triangleBarycentricsDictionary = castValue;
                return true;
            }
            case "m_triangleDictionaryEntries":
            case "triangleDictionaryEntries":
            {
                if (value is not List<hclVirtualCollisionPointsData.BarycentricDictionaryEntry> castValue) return false;
                instance.m_triangleDictionaryEntries = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<hclVirtualCollisionPointsData.EdgeFanSection> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_edgeFans":
            case "edgeFans":
            {
                if (value is not List<hclVirtualCollisionPointsData.EdgeFan> castValue) return false;
                instance.m_edgeFans = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<hclVirtualCollisionPointsData.TriangleFanSection> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_triangleFans":
            case "triangleFans":
            {
                if (value is not List<hclVirtualCollisionPointsData.TriangleFan> castValue) return false;
                instance.m_triangleFans = castValue;
                return true;
            }
            case "m_edgesLandscape":
            case "edgesLandscape":
            {
                if (value is not List<hclVirtualCollisionPointsData.EdgeFanSection> castValue) return false;
                instance.m_edgesLandscape = castValue;
                return true;
            }
            case "m_edgeFansLandscape":
            case "edgeFansLandscape":
            {
                if (value is not List<hclVirtualCollisionPointsData.EdgeFanLandscape> castValue) return false;
                instance.m_edgeFansLandscape = castValue;
                return true;
            }
            case "m_trianglesLandscape":
            case "trianglesLandscape":
            {
                if (value is not List<hclVirtualCollisionPointsData.TriangleFanSection> castValue) return false;
                instance.m_trianglesLandscape = castValue;
                return true;
            }
            case "m_triangleFansLandscape":
            case "triangleFansLandscape":
            {
                if (value is not List<hclVirtualCollisionPointsData.TriangleFanLandscape> castValue) return false;
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
