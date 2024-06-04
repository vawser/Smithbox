// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSimulationSetupMeshMapOptionsData : HavokData<hclSimulationSetupMeshMapOptions> 
{
    public hclSimulationSetupMeshMapOptionsData(HavokType type, hclSimulationSetupMeshMapOptions instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collapseVertices":
            case "collapseVertices":
            {
                if (instance.m_collapseVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collapseThreshold":
            case "collapseThreshold":
            {
                if (instance.m_collapseThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (instance.m_vertexSelection is not TGet castValue) return false;
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
            case "m_collapseVertices":
            case "collapseVertices":
            {
                if (value is not bool castValue) return false;
                instance.m_collapseVertices = castValue;
                return true;
            }
            case "m_collapseThreshold":
            case "collapseThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_collapseThreshold = castValue;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
