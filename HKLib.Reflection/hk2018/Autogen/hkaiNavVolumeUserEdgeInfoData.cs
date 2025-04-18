// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeUserEdgeInfoData : HavokData<hkaiNavVolume.UserEdgeInfo> 
{
    private static readonly System.Reflection.FieldInfo _entryOriginInfo = typeof(hkaiNavVolume.UserEdgeInfo).GetField("m_entryOrigin")!;
    private static readonly System.Reflection.FieldInfo _exitOriginInfo = typeof(hkaiNavVolume.UserEdgeInfo).GetField("m_exitOrigin")!;
    public hkaiNavVolumeUserEdgeInfoData(HavokType type, hkaiNavVolume.UserEdgeInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entryOrigin":
            case "entryOrigin":
            {
                if (instance.m_entryOrigin is not TGet castValue) return false;
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
            case "m_exitOrigin":
            case "exitOrigin":
            {
                if (instance.m_exitOrigin is not TGet castValue) return false;
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
            case "m_entryAxisPermutation":
            case "entryAxisPermutation":
            {
                if (instance.m_entryAxisPermutation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_exitAxisPermutation":
            case "exitAxisPermutation":
            {
                if (instance.m_exitAxisPermutation is not TGet castValue) return false;
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
            case "m_entryOrigin":
            case "entryOrigin":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _entryOriginInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_uExtent":
            case "uExtent":
            {
                if (value is not ushort castValue) return false;
                instance.m_uExtent = castValue;
                return true;
            }
            case "m_exitOrigin":
            case "exitOrigin":
            {
                if (value is not ushort[] castValue || castValue.Length != 3) return false;
                try
                {
                    _exitOriginInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_vExtent":
            case "vExtent":
            {
                if (value is not ushort castValue) return false;
                instance.m_vExtent = castValue;
                return true;
            }
            case "m_entryAxisPermutation":
            case "entryAxisPermutation":
            {
                if (value is not hkaiNavVolume.AxisPermutation castValue) return false;
                instance.m_entryAxisPermutation = castValue;
                return true;
            }
            case "m_exitAxisPermutation":
            case "exitAxisPermutation":
            {
                if (value is not hkaiNavVolume.AxisPermutation castValue) return false;
                instance.m_exitAxisPermutation = castValue;
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
