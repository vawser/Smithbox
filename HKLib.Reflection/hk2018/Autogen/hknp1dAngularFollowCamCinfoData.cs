// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknp1dAngularFollowCamCinfoData : HavokData<hknp1dAngularFollowCamCinfo> 
{
    public hknp1dAngularFollowCamCinfoData(HavokType type, hknp1dAngularFollowCamCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_yawCorrection":
            case "yawCorrection":
            {
                if (instance.m_yawCorrection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_yawSignCorrection":
            case "yawSignCorrection":
            {
                if (instance.m_yawSignCorrection is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_upDirWS":
            case "upDirWS":
            {
                if (instance.m_upDirWS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rigidBodyForwardDir":
            case "rigidBodyForwardDir":
            {
                if (instance.m_rigidBodyForwardDir is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_set":
            case "set":
            {
                if (instance.m_set is not TGet castValue) return false;
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
                if (value is not List<hknp1dAngularFollowCamCinfo.CameraSet> castValue) return false;
                instance.m_set = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
