// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpCharacterSurfaceInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpCharacterSurfaceInfo>
{
    public hknpCharacterSurfaceInfoData(Havok.hk2019.hknpCharacterSurfaceInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_isSurfaceDynamic":
            case "isSurfaceDynamic":
            {
                value = instance.m_isSurfaceDynamic;
                return true;
            }
            case "m_supportedState":
            case "supportedState":
            {
                value = instance.m_supportedState;
                return true;
            }
            case "m_surfaceDistanceExcess":
            case "surfaceDistanceExcess":
            {
                value = instance.m_surfaceDistanceExcess;
                return true;
            }
            case "m_surfaceNormal":
            case "surfaceNormal":
            {
                value = instance.m_surfaceNormal;
                return true;
            }
            case "m_surfaceVelocity":
            case "surfaceVelocity":
            {
                value = instance.m_surfaceVelocity;
                return true;
            }
            case "m_surfaceAngularVelocity":
            case "surfaceAngularVelocity":
            {
                value = instance.m_surfaceAngularVelocity;
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
                if (value is Havok.hk2019.hknpCharacterSurfaceInfo.SupportedState castValue)
                {
                    instance.m_supportedState = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_supportedState = (Havok.hk2019.hknpCharacterSurfaceInfo.SupportedState)byteValue;
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
