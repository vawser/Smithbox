// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclLocalRangeSetupObjectData : HavokData<hclLocalRangeSetupObject> 
{
    public hclLocalRangeSetupObjectData(HavokType type, hclLocalRangeSetupObject instance) : base(type, instance) {}

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
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (instance.m_referenceBufferSetup is null)
                {
                    return true;
                }
                if (instance.m_referenceBufferSetup is TGet castValue)
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
            case "m_maximumDistance":
            case "maximumDistance":
            {
                if (instance.m_maximumDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                if (instance.m_minNormalDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                if (instance.m_maxNormalDistance is not TGet castValue) return false;
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
            case "m_localRangeShape":
            case "localRangeShape":
            {
                if (instance.m_localRangeShape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_localRangeShape is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_useMinNormalDistance":
            case "useMinNormalDistance":
            {
                if (instance.m_useMinNormalDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useMaxNormalDistance":
            case "useMaxNormalDistance":
            {
                if (instance.m_useMaxNormalDistance is not TGet castValue) return false;
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
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (value is null)
                {
                    instance.m_referenceBufferSetup = default;
                    return true;
                }
                if (value is hclBufferSetupObject castValue)
                {
                    instance.m_referenceBufferSetup = castValue;
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
            case "m_maximumDistance":
            case "maximumDistance":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_maximumDistance = castValue;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_minNormalDistance = castValue;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                if (value is not hclVertexFloatInput castValue) return false;
                instance.m_maxNormalDistance = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_localRangeShape":
            case "localRangeShape":
            {
                if (value is hclLocalRangeConstraintSet.ShapeType castValue)
                {
                    instance.m_localRangeShape = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_localRangeShape = (hclLocalRangeConstraintSet.ShapeType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_useMinNormalDistance":
            case "useMinNormalDistance":
            {
                if (value is not bool castValue) return false;
                instance.m_useMinNormalDistance = castValue;
                return true;
            }
            case "m_useMaxNormalDistance":
            case "useMaxNormalDistance":
            {
                if (value is not bool castValue) return false;
                instance.m_useMaxNormalDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
