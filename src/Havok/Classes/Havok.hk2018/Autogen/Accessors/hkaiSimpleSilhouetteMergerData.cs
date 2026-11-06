// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiSimpleSilhouetteMergerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiSimpleSilhouetteMerger>
{
    public hkaiSimpleSilhouetteMergerData(Havok.hk2018.hkaiSimpleSilhouetteMerger instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_mergeType":
            case "mergeType":
            {
                if (value is Havok.hk2018.hkaiSilhouetteMerger.MergeType castValue)
                {
                    instance.m_mergeType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_mergeType = (Havok.hk2018.hkaiSilhouetteMerger.MergeType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_mergeParams":
            case "mergeParams":
            {
                if (value is not Havok.hk2018.hkaiSilhouetteGenerationParameters castValue) return false;
                instance.m_mergeParams = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
