// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStandardLinkSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStandardLinkSetupObject>
{
    public hclStandardLinkSetupObjectData(Havok.hk2014.hclStandardLinkSetupObject instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_simulationMesh":
            case "simulationMesh":
            {
                value = instance.m_simulationMesh;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
                return true;
            }
            case "m_edgeSelection":
            case "edgeSelection":
            {
                value = instance.m_edgeSelection;
                return true;
            }
            case "m_ignoreHiddenEdges":
            case "ignoreHiddenEdges":
            {
                value = instance.m_ignoreHiddenEdges;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
                return true;
            }
            case "m_allowedCompression":
            case "allowedCompression":
            {
                value = instance.m_allowedCompression;
                return true;
            }
            case "m_allowedStretching":
            case "allowedStretching":
            {
                value = instance.m_allowedStretching;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (value is null)
                {
                    instance.m_simulationMesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hclSimulationSetupMesh castValue)
                {
                    instance.m_simulationMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not Havok.hk2014.hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_edgeSelection":
            case "edgeSelection":
            {
                if (value is not Havok.hk2014.hclEdgeSelectionInput castValue) return false;
                instance.m_edgeSelection = castValue;
                return true;
            }
            case "m_ignoreHiddenEdges":
            case "ignoreHiddenEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_ignoreHiddenEdges = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_allowedCompression":
            case "allowedCompression":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_allowedCompression = castValue;
                return true;
            }
            case "m_allowedStretching":
            case "allowedStretching":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_allowedStretching = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
