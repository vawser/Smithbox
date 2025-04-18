// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBendLinkSetupObjectData : HavokData<hclBendLinkSetupObject> 
{
    public hclBendLinkSetupObjectData(HavokType type, hclBendLinkSetupObject instance) : base(type, instance) {}

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
            case "m_createStandardLinks":
            case "createStandardLinks":
            {
                if (instance.m_createStandardLinks is not TGet castValue) return false;
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
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (instance.m_bendStiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stretchStiffness":
            case "stretchStiffness":
            {
                if (instance.m_stretchStiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flatnessFactor":
            case "flatnessFactor":
            {
                if (instance.m_flatnessFactor is not TGet castValue) return false;
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
            case "m_createStandardLinks":
            case "createStandardLinks":
            {
                if (value is not bool castValue) return false;
                instance.m_createStandardLinks = castValue;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                if (value is not hclVertexSelectionInput castValue) return false;
                instance.m_vertexSelection = castValue;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_bendStiffness = castValue;
                return true;
            }
            case "m_stretchStiffness":
            case "stretchStiffness":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_stretchStiffness = castValue;
                return true;
            }
            case "m_flatnessFactor":
            case "flatnessFactor":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_flatnessFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
