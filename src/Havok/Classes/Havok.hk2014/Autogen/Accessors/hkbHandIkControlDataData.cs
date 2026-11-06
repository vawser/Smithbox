// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbHandIkControlDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbHandIkControlData>
{
    public hkbHandIkControlDataData(Havok.hk2014.hkbHandIkControlData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_targetPosition":
            case "targetPosition":
            {
                value = instance.m_targetPosition;
                return true;
            }
            case "m_targetRotation":
            case "targetRotation":
            {
                value = instance.m_targetRotation;
                return true;
            }
            case "m_targetNormal":
            case "targetNormal":
            {
                value = instance.m_targetNormal;
                return true;
            }
            case "m_targetHandle":
            case "targetHandle":
            {
                value = instance.m_targetHandle;
                return true;
            }
            case "m_transformOnFraction":
            case "transformOnFraction":
            {
                value = instance.m_transformOnFraction;
                return true;
            }
            case "m_normalOnFraction":
            case "normalOnFraction":
            {
                value = instance.m_normalOnFraction;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                value = instance.m_fadeInDuration;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                value = instance.m_extrapolationTimeStep;
                return true;
            }
            case "m_handleChangeSpeed":
            case "handleChangeSpeed":
            {
                value = instance.m_handleChangeSpeed;
                return true;
            }
            case "m_handleChangeMode":
            case "handleChangeMode":
            {
                value = instance.m_handleChangeMode;
                return true;
            }
            case "m_fixUp":
            case "fixUp":
            {
                value = instance.m_fixUp;
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
            case "m_targetPosition":
            case "targetPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetPosition = castValue;
                return true;
            }
            case "m_targetRotation":
            case "targetRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_targetRotation = castValue;
                return true;
            }
            case "m_targetNormal":
            case "targetNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetNormal = castValue;
                return true;
            }
            case "m_targetHandle":
            case "targetHandle":
            {
                if (value is null)
                {
                    instance.m_targetHandle = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbHandle castValue)
                {
                    instance.m_targetHandle = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformOnFraction":
            case "transformOnFraction":
            {
                if (value is not float castValue) return false;
                instance.m_transformOnFraction = castValue;
                return true;
            }
            case "m_normalOnFraction":
            case "normalOnFraction":
            {
                if (value is not float castValue) return false;
                instance.m_normalOnFraction = castValue;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInDuration = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                if (value is not float castValue) return false;
                instance.m_extrapolationTimeStep = castValue;
                return true;
            }
            case "m_handleChangeSpeed":
            case "handleChangeSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_handleChangeSpeed = castValue;
                return true;
            }
            case "m_handleChangeMode":
            case "handleChangeMode":
            {
                if (value is Havok.hk2014.hkbHandIkControlData.HandleChangeMode castValue)
                {
                    instance.m_handleChangeMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_handleChangeMode = (Havok.hk2014.hkbHandIkControlData.HandleChangeMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_fixUp":
            case "fixUp":
            {
                if (value is not bool castValue) return false;
                instance.m_fixUp = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
