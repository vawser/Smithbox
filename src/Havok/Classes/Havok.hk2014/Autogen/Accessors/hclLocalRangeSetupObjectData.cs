// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclLocalRangeSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclLocalRangeSetupObject>
{
    public hclLocalRangeSetupObjectData(Havok.hk2014.hclLocalRangeSetupObject instance) : base(instance) { }

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
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                value = instance.m_referenceBufferSetup;
                return true;
            }
            case "m_vertexSelection":
            case "vertexSelection":
            {
                value = instance.m_vertexSelection;
                return true;
            }
            case "m_maximumDistance":
            case "maximumDistance":
            {
                value = instance.m_maximumDistance;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                value = instance.m_minNormalDistance;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                value = instance.m_maxNormalDistance;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
                return true;
            }
            case "m_localRangeShape":
            case "localRangeShape":
            {
                value = instance.m_localRangeShape;
                return true;
            }
            case "m_useMinNormalDistance":
            case "useMinNormalDistance":
            {
                value = instance.m_useMinNormalDistance;
                return true;
            }
            case "m_useMaxNormalDistance":
            case "useMaxNormalDistance":
            {
                value = instance.m_useMaxNormalDistance;
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
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                if (value is null)
                {
                    instance.m_referenceBufferSetup = default;
                    return true;
                }
                if (value is Havok.hk2014.hclBufferSetupObject castValue)
                {
                    instance.m_referenceBufferSetup = castValue;
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
            case "m_maximumDistance":
            case "maximumDistance":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_maximumDistance = castValue;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_minNormalDistance = castValue;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
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
                if (value is Havok.hk2014.hclLocalRangeConstraintSet.ShapeType castValue)
                {
                    instance.m_localRangeShape = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_localRangeShape = (Havok.hk2014.hclLocalRangeConstraintSet.ShapeType)uintValue;
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
