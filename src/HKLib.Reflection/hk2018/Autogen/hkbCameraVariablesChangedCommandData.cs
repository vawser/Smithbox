// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCameraVariablesChangedCommandData : HavokData<hkbCameraVariablesChangedCommand> 
{
    public hkbCameraVariablesChangedCommandData(HavokType type, hkbCameraVariablesChangedCommand instance) : base(type, instance) {}

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
            case "m_cameraVariableFloatNames":
            case "cameraVariableFloatNames":
            {
                if (instance.m_cameraVariableFloatNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cameraFloatValues":
            case "cameraFloatValues":
            {
                if (instance.m_cameraFloatValues is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cameraVariableVectorNames":
            case "cameraVariableVectorNames":
            {
                if (instance.m_cameraVariableVectorNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cameraVectorValues":
            case "cameraVectorValues":
            {
                if (instance.m_cameraVectorValues is not TGet castValue) return false;
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
            case "m_cameraVariableFloatNames":
            case "cameraVariableFloatNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_cameraVariableFloatNames = castValue;
                return true;
            }
            case "m_cameraFloatValues":
            case "cameraFloatValues":
            {
                if (value is not List<float> castValue) return false;
                instance.m_cameraFloatValues = castValue;
                return true;
            }
            case "m_cameraVariableVectorNames":
            case "cameraVariableVectorNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_cameraVariableVectorNames = castValue;
                return true;
            }
            case "m_cameraVectorValues":
            case "cameraVectorValues":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_cameraVectorValues = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
