// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbHandIkControlDataData : HavokData<hkbHandIkControlData> 
{
    public hkbHandIkControlDataData(HavokType type, hkbHandIkControlData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_targetPosition":
            case "targetPosition":
            {
                if (instance.m_targetPosition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetRotation":
            case "targetRotation":
            {
                if (instance.m_targetRotation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetNormal":
            case "targetNormal":
            {
                if (instance.m_targetNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_targetHandle":
            case "targetHandle":
            {
                if (instance.m_targetHandle is null)
                {
                    return true;
                }
                if (instance.m_targetHandle is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformOnFraction":
            case "transformOnFraction":
            {
                if (instance.m_transformOnFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normalOnFraction":
            case "normalOnFraction":
            {
                if (instance.m_normalOnFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (instance.m_fadeInDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (instance.m_fadeOutDuration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                if (instance.m_extrapolationTimeStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleChangeSpeed":
            case "handleChangeSpeed":
            {
                if (instance.m_handleChangeSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleChangeMode":
            case "handleChangeMode":
            {
                if (instance.m_handleChangeMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_handleChangeMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_fixUp":
            case "fixUp":
            {
                if (instance.m_fixUp is not TGet castValue) return false;
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
                if (value is hkbHandle castValue)
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
                if (value is hkbHandIkControlData.HandleChangeMode castValue)
                {
                    instance.m_handleChangeMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_handleChangeMode = (hkbHandIkControlData.HandleChangeMode)sbyteValue;
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
