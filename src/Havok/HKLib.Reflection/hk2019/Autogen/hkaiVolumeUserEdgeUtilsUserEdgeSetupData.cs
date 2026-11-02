// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiVolumeUserEdgeUtilsUserEdgeSetupData : HavokData<HKLib.hk2019.hkaiVolumeUserEdgeUtils.UserEdgeSetup> 
{
    public hkaiVolumeUserEdgeUtilsUserEdgeSetupData(HavokType type, HKLib.hk2019.hkaiVolumeUserEdgeUtils.UserEdgeSetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entryPortal":
            case "entryPortal":
            {
                if (instance.m_entryPortal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_exitPortal":
            case "exitPortal":
            {
                if (instance.m_exitPortal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeData":
            case "userEdgeData":
            {
                if (instance.m_userEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cost":
            case "cost":
            {
                if (instance.m_cost is not TGet castValue) return false;
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
            case "m_entryPortal":
            case "entryPortal":
            {
                if (value is not HKLib.hk2019.hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal castValue) return false;
                instance.m_entryPortal = castValue;
                return true;
            }
            case "m_exitPortal":
            case "exitPortal":
            {
                if (value is not HKLib.hk2019.hkaiVolumeUserEdgeUtils.UserEdgeSetup.Portal castValue) return false;
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
