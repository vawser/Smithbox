// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpBodyQualityLibraryData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpBodyQualityLibrary>
{
    private static readonly System.Reflection.FieldInfo _qualitiesInfo = typeof(Havok.hk2014.hknpBodyQualityLibrary).GetField("m_qualities")!;
    public hknpBodyQualityLibraryData(Havok.hk2014.hknpBodyQualityLibrary instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_qualities":
            case "qualities":
            {
                if (value is not Havok.hk2014.hknpBodyQuality[] castValue || castValue.Length != 32) return false;
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
