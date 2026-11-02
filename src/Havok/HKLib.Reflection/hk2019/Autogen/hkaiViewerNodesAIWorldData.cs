// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiViewerNodesAIWorldData : HavokData<HKLib.hk2019.hkaiViewerNodes.AIWorld> 
{
    private static readonly System.Reflection.FieldInfo _layersInfo = typeof(HKLib.hk2019.hkaiViewerNodes.AIWorld).GetField("m_layers")!;
    public hkaiViewerNodesAIWorldData(HavokType type, HKLib.hk2019.hkaiViewerNodes.AIWorld instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceResetMethod":
            case "clearanceResetMethod":
            {
                if (instance.m_clearanceResetMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_clearanceResetMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_maxCutFacesPerStep":
            case "maxCutFacesPerStep":
            {
                if (instance.m_maxCutFacesPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxRequestsPerStep":
            case "maxRequestsPerStep":
            {
                if (instance.m_maxRequestsPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxIterationsPerStep":
            case "maxIterationsPerStep":
            {
                if (instance.m_maxIterationsPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceFillFacesPerStep":
            case "clearanceFillFacesPerStep":
            {
                if (instance.m_clearanceFillFacesPerStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_priorityThreshold":
            case "priorityThreshold":
            {
                if (instance.m_priorityThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layers":
            case "layers":
            {
                if (instance.m_layers is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkaiWorld.ClearanceResetMethod castValue)
                {
                    instance.m_clearanceResetMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_clearanceResetMethod = (HKLib.hk2019.hkaiWorld.ClearanceResetMethod)byteValue;
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
                if (value is not HKLib.hk2019.hkaiViewerNodes.Layer[] castValue || castValue.Length != 32) return false;
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
