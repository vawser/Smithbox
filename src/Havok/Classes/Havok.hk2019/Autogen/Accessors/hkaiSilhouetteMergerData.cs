// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiSilhouetteMergerData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiSilhouetteMerger>
{
    public hkaiSilhouetteMergerData(Havok.hk2019.hkaiSilhouetteMerger instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mergeType":
            case "mergeType":
            {
                value = instance.m_mergeType;
                return true;
            }
            case "m_mergeParams":
            case "mergeParams":
            {
                value = instance.m_mergeParams;
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
            case "m_mergeType":
            case "mergeType":
            {
                if (value is Havok.hk2019.hkaiSilhouetteMerger.MergeType castValue)
                {
                    instance.m_mergeType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_mergeType = (Havok.hk2019.hkaiSilhouetteMerger.MergeType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_mergeParams":
            case "mergeParams":
            {
                if (value is not Havok.hk2019.hkaiSilhouetteGenerationParameters castValue) return false;
                instance.m_mergeParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
