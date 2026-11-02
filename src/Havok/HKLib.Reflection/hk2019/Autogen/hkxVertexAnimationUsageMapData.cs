// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxVertexAnimationUsageMapData : HavokData<HKLib.hk2019.hkxVertexAnimation.UsageMap> 
{
    public hkxVertexAnimationUsageMapData(HavokType type, HKLib.hk2019.hkxVertexAnimation.UsageMap instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_use":
            case "use":
            {
                if (instance.m_use is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_use is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_useIndexOrig":
            case "useIndexOrig":
            {
                if (instance.m_useIndexOrig is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useIndexLocal":
            case "useIndexLocal":
            {
                if (instance.m_useIndexLocal is not TGet castValue) return false;
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
            case "m_use":
            case "use":
            {
                if (value is HKLib.hk2019.hkxVertexDescription.DataUsage castValue)
                {
                    instance.m_use = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_use = (HKLib.hk2019.hkxVertexDescription.DataUsage)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_useIndexOrig":
            case "useIndexOrig":
            {
                if (value is not byte castValue) return false;
                instance.m_useIndexOrig = castValue;
                return true;
            }
            case "m_useIndexLocal":
            case "useIndexLocal":
            {
                if (value is not byte castValue) return false;
                instance.m_useIndexLocal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
