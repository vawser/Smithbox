// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiVolumeUserEdgeUtilsUserEdgeSetupData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiVolumeUserEdgeUtils.UserEdgeSetup>
{
    public hkaiVolumeUserEdgeUtilsUserEdgeSetupData(Havok.hk2018.hkaiVolumeUserEdgeUtils.UserEdgeSetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entryPortal":
            case "entryPortal":
            {
                value = instance.m_entryPortal;
                return true;
            }
            case "m_exitPortal":
            case "exitPortal":
            {
                value = instance.m_exitPortal;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                value = instance.m_userEdgeData;
                return true;
            }
            case "m_cost":
            case "cost":
            {
                value = instance.m_cost;
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
            case "m_entryPortal":
            case "entryPortal":
            {
                if (value is not Havok.hk2018.hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal castValue) return false;
                instance.m_entryPortal = castValue;
                return true;
            }
            case "m_exitPortal":
            case "exitPortal":
            {
                if (value is not Havok.hk2018.hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal castValue) return false;
                instance.m_exitPortal = castValue;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                if (value is not int castValue) return false;
                instance.m_userEdgeData = castValue;
                return true;
            }
            case "m_cost":
            case "cost":
            {
                if (value is not float castValue) return false;
                instance.m_cost = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
