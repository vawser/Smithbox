// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiViewerNodesAIWorldData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiViewerNodes.AIWorld>
{
    private static readonly System.Reflection.FieldInfo _layersInfo = typeof(Havok.hk2019.hkaiViewerNodes.AIWorld).GetField("m_layers")!;
    public hkaiViewerNodesAIWorldData(Havok.hk2019.hkaiViewerNodes.AIWorld instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                value = instance.m_clearanceResetMethod;
                return true;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                value = instance.m_maxCutFacesPerStep;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                value = instance.m_maxRequestsPerStep;
                return true;
            }
            case "m_maxIterationsPerStep":
            case "maxIterationsPerStep":
            {
                value = instance.m_maxIterationsPerStep;
                return true;
            }
            case "m_clearanceFillFacesPerStep":
            case "clearanceFillFacesPerStep":
            {
                value = instance.m_clearanceFillFacesPerStep;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                value = instance.m_priorityThreshold;
                return true;
            }
            case "m_layers":
            case "layers":
            {
                value = instance.m_layers;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                if (value is Havok.hk2019.hkaiWorld.ClearanceResetMethod castValue)
                {
                    instance.m_clearanceResetMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_clearanceResetMethod = (Havok.hk2019.hkaiWorld.ClearanceResetMethod)byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxCutFacesPerStep = castValue;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxRequestsPerStep = castValue;
                return true;
            }
            case "m_maxIterationsPerStep":
            case "maxIterationsPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_maxIterationsPerStep = castValue;
                return true;
            }
            case "m_clearanceFillFacesPerStep":
            case "clearanceFillFacesPerStep":
            {
                if (value is not int castValue) return false;
                instance.m_clearanceFillFacesPerStep = castValue;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                if (value is not int castValue) return false;
                instance.m_priorityThreshold = castValue;
                return true;
            }
            case "m_layers":
            case "layers":
            {
                if (value is not Havok.hk2019.hkaiViewerNodes.Layer[] castValue || castValue.Length != 32) return false;
                try
                {
                    _layersInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
