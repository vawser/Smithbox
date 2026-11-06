// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkcdStaticMeshTreeBaseSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkcdStaticMeshTreeBase.Section>
{
    private static readonly System.Reflection.FieldInfo _codecParmsInfo = typeof(Havok.hk2016.hkcdStaticMeshTreeBase.Section).GetField("m_codecParms")!;
    public hkcdStaticMeshTreeBaseSectionData(Havok.hk2016.hkcdStaticMeshTreeBase.Section instance) : base(instance) { }

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
            case "m_firstPackedVertex":
            case "firstPackedVertex":
            {
                value = instance.m_firstPackedVertex;
                return true;
            }
            case "m_sharedVertices":
            case "sharedVertices":
            {
                value = instance.m_sharedVertices;
                return true;
            }
            case "m_primitives":
            case "primitives":
            {
                value = instance.m_primitives;
                return true;
            }
            case "m_dataRuns":
            case "dataRuns":
            {
                value = instance.m_dataRuns;
                return true;
            }
            case "m_numPackedVertices":
            case "numPackedVertices":
            {
                value = instance.m_numPackedVertices;
                return true;
            }
            case "m_numSharedIndices":
            case "numSharedIndices":
            {
                value = instance.m_numSharedIndices;
                return true;
            }
            case "m_leafIndex":
            case "leafIndex":
            {
                value = instance.m_leafIndex;
                return true;
            }
            case "m_page":
            case "page":
            {
                value = instance.m_page;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_layerData":
            case "layerData":
            {
                value = instance.m_layerData;
                return true;
            }
            case "m_unusedData":
            case "unusedData":
            {
                value = instance.m_unusedData;
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
                if (value is not List<Havok.hk2016.hkcdStaticTree.Codec3Axis4> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2016.hkAabb castValue) return false;
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
            case "m_firstPackedVertex":
            case "firstPackedVertex":
            {
                if (value is not uint castValue) return false;
                instance.m_firstPackedVertex = castValue;
                return true;
            }
            case "m_sharedVertices":
            case "sharedVertices":
            {
                if (value is not Havok.hk2016.hkcdStaticMeshTreeBase.Section.SharedVertices castValue) return false;
                instance.m_sharedVertices = castValue;
                return true;
            }
            case "m_primitives":
            case "primitives":
            {
                if (value is not Havok.hk2016.hkcdStaticMeshTreeBase.Section.Primitives castValue) return false;
                instance.m_primitives = castValue;
                return true;
            }
            case "m_dataRuns":
            case "dataRuns":
            {
                if (value is not Havok.hk2016.hkcdStaticMeshTreeBase.Section.DataRuns castValue) return false;
                instance.m_dataRuns = castValue;
                return true;
            }
            case "m_numPackedVertices":
            case "numPackedVertices":
            {
                if (value is not byte castValue) return false;
                instance.m_numPackedVertices = castValue;
                return true;
            }
            case "m_numSharedIndices":
            case "numSharedIndices":
            {
                if (value is not byte castValue) return false;
                instance.m_numSharedIndices = castValue;
                return true;
            }
            case "m_leafIndex":
            case "leafIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_leafIndex = castValue;
                return true;
            }
            case "m_page":
            case "page":
            {
                if (value is not byte castValue) return false;
                instance.m_page = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not byte castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_layerData":
            case "layerData":
            {
                if (value is not byte castValue) return false;
                instance.m_layerData = castValue;
                return true;
            }
            case "m_unusedData":
            case "unusedData":
            {
                if (value is not byte castValue) return false;
                instance.m_unusedData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
