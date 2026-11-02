// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpBodyQualityLibraryData : HavokData<HKLib.hk2019.hknpBodyQualityLibrary> 
{
    private static readonly System.Reflection.FieldInfo _qualitiesInfo = typeof(HKLib.hk2019.hknpBodyQualityLibrary).GetField("m_qualities")!;
    public hknpBodyQualityLibraryData(HavokType type, HKLib.hk2019.hknpBodyQualityLibrary instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_qualities":
            case "qualities":
            {
                if (value is not HKLib.hk2019.hknpBodyQuality[] castValue || castValue.Length != 32) return false;
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
