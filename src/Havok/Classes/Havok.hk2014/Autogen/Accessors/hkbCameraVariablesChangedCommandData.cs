// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCameraVariablesChangedCommandData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCameraVariablesChangedCommand>
{
    public hkbCameraVariablesChangedCommandData(Havok.hk2014.hkbCameraVariablesChangedCommand instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cameraVariableFloatNames":
            case "cameraVariableFloatNames":
            {
                value = instance.m_cameraVariableFloatNames;
                return true;
            }
            case "m_cameraFloatValues":
            case "cameraFloatValues":
            {
                value = instance.m_cameraFloatValues;
                return true;
            }
            case "m_cameraVariableVectorNames":
            case "cameraVariableVectorNames":
            {
                value = instance.m_cameraVariableVectorNames;
                return true;
            }
            case "m_cameraVectorValues":
            case "cameraVectorValues":
            {
                value = instance.m_cameraVectorValues;
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
