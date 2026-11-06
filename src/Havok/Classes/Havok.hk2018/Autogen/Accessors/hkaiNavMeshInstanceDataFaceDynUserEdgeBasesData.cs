// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshInstanceDataFaceDynUserEdgeBasesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshInstanceData.FaceDynUserEdgeBases>
{
    public hkaiNavMeshInstanceDataFaceDynUserEdgeBasesData(Havok.hk2018.hkaiNavMeshInstanceData.FaceDynUserEdgeBases instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                value = instance.m_edgeData;
                return true;
            }
            case "m_setIds":
            case "setIds":
            {
                value = instance.m_setIds;
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
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2018.hkaiNavMesh.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (value is not List<int> castValue) return false;
                instance.m_edgeData = castValue;
                return true;
            }
            case "m_setIds":
            case "setIds":
            {
                if (value is not List<Havok.hk2018.hkHandle<uint>> castValue) return false;
                instance.m_setIds = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
