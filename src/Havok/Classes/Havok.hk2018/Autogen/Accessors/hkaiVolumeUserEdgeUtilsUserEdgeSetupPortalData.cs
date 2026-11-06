// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiVolumeUserEdgeUtilsUserEdgeSetupPortalData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal>
{
    public hkaiVolumeUserEdgeUtilsUserEdgeSetupPortalData(Havok.hk2018.hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_origin":
            case "origin":
            {
                value = instance.m_origin;
                return true;
            }
            case "m_uExtent":
            case "uExtent":
            {
                value = instance.m_uExtent;
                return true;
            }
            case "m_vExtent":
            case "vExtent":
            {
                value = instance.m_vExtent;
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
