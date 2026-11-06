// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpBodyQualityLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpBodyQualityLibrary>
{
    private static readonly System.Reflection.FieldInfo _qualitiesInfo = typeof(Havok.hk2016.hknpBodyQualityLibrary).GetField("m_qualities")!;
    public hknpBodyQualityLibraryData(Havok.hk2016.hknpBodyQualityLibrary instance) : base(instance) { }

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
            case "m_qualities":
            case "qualities":
            {
                value = instance.m_qualities;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_qualities":
            case "qualities":
            {
                if (value is not Havok.hk2016.hknpBodyQuality[] castValue || castValue.Length != 32) return false;
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
