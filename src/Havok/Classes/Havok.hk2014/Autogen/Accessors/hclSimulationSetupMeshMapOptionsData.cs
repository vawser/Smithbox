// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSimulationSetupMeshMapOptionsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSimulationSetupMeshMapOptions>
{
    public hclSimulationSetupMeshMapOptionsData(Havok.hk2014.hclSimulationSetupMeshMapOptions instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_collapseVertices":
            case "collapseVertices":
            {
                value = instance.m_collapseVertices;
                return true;
            }
            case "m_collapseThreshold":
            case "collapseThreshold":
            {
                value = instance.m_collapseThreshold;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
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
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
