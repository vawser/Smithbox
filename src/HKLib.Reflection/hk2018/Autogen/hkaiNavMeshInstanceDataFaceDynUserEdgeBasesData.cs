// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshInstanceDataFaceDynUserEdgeBasesData : HavokData<HKLib.hk2018.hkaiNavMeshInstanceData.FaceDynUserEdgeBases> 
{
    public hkaiNavMeshInstanceDataFaceDynUserEdgeBasesData(HavokType type, HKLib.hk2018.hkaiNavMeshInstanceData.FaceDynUserEdgeBases instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (instance.m_edgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setIds":
            case "setIds":
            {
                if (instance.m_setIds is not TGet castValue) return false;
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
            case "m_edges":
            case "edges":
            {
                if (value is not List<hkaiNavMesh.Edge> castValue) return false;
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
                if (value is not List<hkHandle<uint>> castValue) return false;
                instance.m_setIds = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
