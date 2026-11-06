// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclTransitionSetupObjectData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclTransitionSetupObject>
{
    public hclTransitionSetupObjectData(Havok.hk2014.hclTransitionSetupObject instance) : base(instance) { }

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
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                value = instance.m_toAnimDelay;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                value = instance.m_toSimDelay;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                value = instance.m_toSimMaxDistance;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                value = instance.m_toAnimPeriod;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                value = instance.m_toSimPeriod;
                return true;
            }
            case "m_referenceBufferSetup":
            case "referenceBufferSetup":
            {
                value = instance.m_referenceBufferSetup;
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
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_toAnimDelay = castValue;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_toSimDelay = castValue;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                if (value is not Havok.hk2014.hclVertexFloatInput castValue) return false;
                instance.m_toSimMaxDistance = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toAnimPeriod = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toSimPeriod = castValue;
                return true;
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
            default:
            return false;
        }
    }

}
