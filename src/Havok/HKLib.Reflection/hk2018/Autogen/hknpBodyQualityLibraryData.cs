// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpBodyQualityLibraryData : HavokData<hknpBodyQualityLibrary> 
{
    private static readonly System.Reflection.FieldInfo _qualitiesInfo = typeof(hknpBodyQualityLibrary).GetField("m_qualities")!;
    public hknpBodyQualityLibraryData(HavokType type, hknpBodyQualityLibrary instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_qualities":
            case "qualities":
            {
                if (instance.m_qualities is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_qualities":
            case "qualities":
            {
                if (value is not hknpBodyQuality[] castValue || castValue.Length != 32) return false;
                try
                {
                    _qualitiesInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
