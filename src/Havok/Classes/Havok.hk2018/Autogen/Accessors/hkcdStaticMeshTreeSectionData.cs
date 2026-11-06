// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdStaticMeshTreeSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdStaticMeshTree.Section>
{
    private static readonly System.Reflection.FieldInfo _codecParmsInfo = typeof(Havok.hk2018.hkcdStaticMeshTree.Section).GetField("m_codecParms")!;
    public hkcdStaticMeshTreeSectionData(Havok.hk2018.hkcdStaticMeshTree.Section instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                value = instance.m_domain;
                return true;
            }
            case "m_codecParms":
            case "codecParms":
            {
                value = instance.m_codecParms;
                return true;
            }
            case "m_firstPackedVertexIndex":
            case "firstPackedVertexIndex":
            {
                value = instance.m_firstPackedVertexIndex;
                return true;
            }
            case "m_firstSharedVertexIndex":
            case "firstSharedVertexIndex":
            {
                value = instance.m_firstSharedVertexIndex;
                return true;
            }
            case "m_firstPrimitiveIndex":
            case "firstPrimitiveIndex":
            {
                value = instance.m_firstPrimitiveIndex;
                return true;
            }
            case "m_firstDataRunIndex":
            case "firstDataRunIndex":
            {
                value = instance.m_firstDataRunIndex;
                return true;
            }
            case "m_numPackedVertices":
            case "numPackedVertices":
            {
                value = instance.m_numPackedVertices;
                return true;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                value = instance.m_numPrimitives;
                return true;
            }
            case "m_numDataRuns":
            case "numDataRuns":
            {
                value = instance.m_numDataRuns;
                return true;
            }
            case "m_page":
            case "page":
            {
                value = instance.m_page;
                return true;
            }
            case "m_leafIndex":
            case "leafIndex":
            {
                value = instance.m_leafIndex;
                return true;
            }
            case "m_layerData":
            case "layerData":
            {
                value = instance.m_layerData;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Havok.hk2018.hkcdCompressedAabbCodecs.Aabb4BytesCodec> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            case "m_codecParms":
            case "codecParms":
            {
                if (value is not float[] castValue || castValue.Length != 6) return false;
                try
                {
                    _codecParmsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_firstPackedVertexIndex":
            case "firstPackedVertexIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstPackedVertexIndex = castValue;
                return true;
            }
            case "m_firstSharedVertexIndex":
            case "firstSharedVertexIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstSharedVertexIndex = castValue;
                return true;
            }
            case "m_firstPrimitiveIndex":
            case "firstPrimitiveIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstPrimitiveIndex = castValue;
                return true;
            }
            case "m_firstDataRunIndex":
            case "firstDataRunIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstDataRunIndex = castValue;
                return true;
            }
            case "m_numPackedVertices":
            case "numPackedVertices":
            {
                if (value is not byte castValue) return false;
                instance.m_numPackedVertices = castValue;
                return true;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                if (value is not byte castValue) return false;
                instance.m_numPrimitives = castValue;
                return true;
            }
            case "m_numDataRuns":
            case "numDataRuns":
            {
                if (value is not byte castValue) return false;
                instance.m_numDataRuns = castValue;
                return true;
            }
            case "m_page":
            case "page":
            {
                if (value is not byte castValue) return false;
                instance.m_page = castValue;
                return true;
            }
            case "m_leafIndex":
            case "leafIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_leafIndex = castValue;
                return true;
            }
            case "m_layerData":
            case "layerData":
            {
                if (value is not byte castValue) return false;
                instance.m_layerData = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not byte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
