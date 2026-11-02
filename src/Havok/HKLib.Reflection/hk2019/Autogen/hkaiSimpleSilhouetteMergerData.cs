// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiSimpleSilhouetteMergerData : HavokData<HKLib.hk2019.hkaiSimpleSilhouetteMerger> 
{
    public hkaiSimpleSilhouetteMergerData(HavokType type, HKLib.hk2019.hkaiSimpleSilhouetteMerger instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mergeType":
            case "mergeType":
            {
                if (instance.m_mergeType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_mergeType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_mergeParams":
            case "mergeParams":
            {
                if (instance.m_mergeParams is not TGet castValue) return false;
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
            case "m_mergeType":
            case "mergeType":
            {
                if (value is HKLib.hk2019.hkaiSilhouetteMerger.MergeType castValue)
                {
                    instance.m_mergeType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_mergeType = (HKLib.hk2019.hkaiSilhouetteMerger.MergeType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_mergeParams":
            case "mergeParams":
            {
                if (value is not HKLib.hk2019.hkaiSilhouetteGenerationParameters castValue) return false;
                instance.m_mergeParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
