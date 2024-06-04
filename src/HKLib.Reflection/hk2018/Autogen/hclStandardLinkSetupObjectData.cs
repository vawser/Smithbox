// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStandardLinkSetupObjectData : HavokData<hclStandardLinkSetupObject> 
{
    public hclStandardLinkSetupObjectData(HavokType type, hclStandardLinkSetupObject instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_simulationMesh":
            case "simulationMesh":
            {
                if (instance.m_simulationMesh is null)
                {
                    return true;
                }
                if (instance.m_simulationMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (instance.m_vertexSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeSelection":
            case "edgeSelection":
            {
                if (instance.m_edgeSelection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ignoreHiddenEdges":
            case "ignoreHiddenEdges":
            {
                if (instance.m_ignoreHiddenEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowedCompression":
            case "allowedCompression":
            {
                if (instance.m_allowedCompression is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allowedStretching":
            case "allowedStretching":
            {
                if (instance.m_allowedStretching is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is hclSimulationSetupMesh castValue)
                {
                    instance.m_simulationMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_edgeSelection":
            case "edgeSelection":
            {
                if (value is not hclEdgeSelectionInput castValue) return false;
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
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_allowedCompression":
            case "allowedCompression":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_allowedCompression = castValue;
                return true;
            }
            case "m_allowedStretching":
            case "allowedStretching":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_allowedStretching = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
