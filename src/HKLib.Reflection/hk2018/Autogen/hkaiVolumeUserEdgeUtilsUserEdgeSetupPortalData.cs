// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiVolumeUserEdgeUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumeUserEdgeUtilsUserEdgeSetupPortalData : HavokData<UserEdgeSetup.Portal> 
{
    public hkaiVolumeUserEdgeUtilsUserEdgeSetupPortalData(HavokType type, UserEdgeSetup.Portal instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_origin":
            case "origin":
            {
                if (instance.m_origin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uExtent":
            case "uExtent":
            {
                if (instance.m_uExtent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vExtent":
            case "vExtent":
            {
                if (instance.m_vExtent is not TGet castValue) return false;
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
            case "m_origin":
            case "origin":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_origin = castValue;
                return true;
            }
            case "m_uExtent":
            case "uExtent":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_uExtent = castValue;
                return true;
            }
            case "m_vExtent":
            case "vExtent":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_vExtent = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
