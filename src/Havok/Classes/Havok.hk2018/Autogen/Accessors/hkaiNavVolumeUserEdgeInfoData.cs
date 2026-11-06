// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumeUserEdgeInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolume.UserEdgeInfo>
{
    private static readonly System.Reflection.FieldInfo _entryOriginInfo = typeof(Havok.hk2018.hkaiNavVolume.UserEdgeInfo).GetField("m_entryOrigin")!;
    private static readonly System.Reflection.FieldInfo _exitOriginInfo = typeof(Havok.hk2018.hkaiNavVolume.UserEdgeInfo).GetField("m_exitOrigin")!;
    public hkaiNavVolumeUserEdgeInfoData(Havok.hk2018.hkaiNavVolume.UserEdgeInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_entryOrigin":
            case "entryOrigin":
            {
                value = instance.m_entryOrigin;
                return true;
            }
            case "m_uExtent":
            case "uExtent":
            {
                value = instance.m_uExtent;
                return true;
            }
            case "m_exitOrigin":
            case "exitOrigin":
            {
                value = instance.m_exitOrigin;
                return true;
            }
            case "m_vExtent":
            case "vExtent":
            {
                value = instance.m_vExtent;
                return true;
            }
            case "m_entryAxisPermutation":
            case "entryAxisPermutation":
            {
                value = instance.m_entryAxisPermutation;
                return true;
            }
            case "m_exitAxisPermutation":
            case "exitAxisPermutation":
            {
                value = instance.m_exitAxisPermutation;
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
                if (value is not Havok.hk2018.hkaiNavVolume.AxisPermutation castValue) return false;
                instance.m_entryAxisPermutation = castValue;
                return true;
            }
            case "m_exitAxisPermutation":
            case "exitAxisPermutation":
            {
                if (value is not Havok.hk2018.hkaiNavVolume.AxisPermutation castValue) return false;
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
