// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpCharacterSurfaceInfoData : HavokData<hknpCharacterSurfaceInfo> 
{
    public hknpCharacterSurfaceInfoData(HavokType type, hknpCharacterSurfaceInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isSurfaceDynamic":
            case "isSurfaceDynamic":
            {
                if (instance.m_isSurfaceDynamic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_supportedState":
            case "supportedState":
            {
                if (instance.m_supportedState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_supportedState is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_surfaceDistanceExcess":
            case "surfaceDistanceExcess":
            {
                if (instance.m_surfaceDistanceExcess is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_surfaceNormal":
            case "surfaceNormal":
            {
                if (instance.m_surfaceNormal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (instance.m_surfaceVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_surfaceAngularVelocity":
            case "surfaceAngularVelocity":
            {
                if (instance.m_surfaceAngularVelocity is not TGet castValue) return false;
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
            case "m_isSurfaceDynamic":
            case "isSurfaceDynamic":
            {
                if (value is not bool castValue) return false;
                instance.m_isSurfaceDynamic = castValue;
                return true;
            }
            case "m_supportedState":
            case "supportedState":
            {
                if (value is hknpCharacterSurfaceInfo.SupportedState castValue)
                {
                    instance.m_supportedState = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_supportedState = (hknpCharacterSurfaceInfo.SupportedState)byteValue;
                    return true;
                }
                return false;
            }
            case "m_surfaceDistanceExcess":
            case "surfaceDistanceExcess":
            {
                if (value is not float castValue) return false;
                instance.m_surfaceDistanceExcess = castValue;
                return true;
            }
            case "m_surfaceNormal":
            case "surfaceNormal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_surfaceNormal = castValue;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_surfaceVelocity = castValue;
                return true;
            }
            case "m_surfaceAngularVelocity":
            case "surfaceAngularVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_surfaceAngularVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
