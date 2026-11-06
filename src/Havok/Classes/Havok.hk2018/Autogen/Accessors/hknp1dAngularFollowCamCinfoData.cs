// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknp1dAngularFollowCamCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknp1dAngularFollowCamCinfo>
{
    public hknp1dAngularFollowCamCinfoData(Havok.hk2018.hknp1dAngularFollowCamCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_yawCorrection":
            case "yawCorrection":
            {
                value = instance.m_yawCorrection;
                return true;
            }
            case "m_yawSignCorrection":
            case "yawSignCorrection":
            {
                value = instance.m_yawSignCorrection;
                return true;
            }
            case "m_upDirWS":
            case "upDirWS":
            {
                value = instance.m_upDirWS;
                return true;
            }
            case "m_rigidBodyForwardDir":
            case "rigidBodyForwardDir":
            {
                value = instance.m_rigidBodyForwardDir;
                return true;
            }
            case "m_set":
            case "set":
            {
                value = instance.m_set;
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
            case "m_yawCorrection":
            case "yawCorrection":
            {
                if (value is not float castValue) return false;
                instance.m_yawCorrection = castValue;
                return true;
            }
            case "m_yawSignCorrection":
            case "yawSignCorrection":
            {
                if (value is not float castValue) return false;
                instance.m_yawSignCorrection = castValue;
                return true;
            }
            case "m_upDirWS":
            case "upDirWS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_upDirWS = castValue;
                return true;
            }
            case "m_rigidBodyForwardDir":
            case "rigidBodyForwardDir":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_rigidBodyForwardDir = castValue;
                return true;
            }
            case "m_set":
            case "set":
            {
                if (value is not List<Havok.hk2018.hknp1dAngularFollowCamCinfo.CameraSet> castValue) return false;
                instance.m_set = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
