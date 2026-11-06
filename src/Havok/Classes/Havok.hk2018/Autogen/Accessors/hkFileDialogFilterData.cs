// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkFileDialogFilterData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hk.FileDialogFilter>
{
    public hkFileDialogFilterData(Havok.hk2018.hk.FileDialogFilter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fileTypeDescription":
            case "fileTypeDescription":
            {
                value = instance.m_fileTypeDescription;
                return true;
            }
            case "m_fileExtensions":
            case "fileExtensions":
            {
                value = instance.m_fileExtensions;
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
            case "m_fileTypeDescription":
            case "fileTypeDescription":
            {
                if (value is null)
                {
                    instance.m_fileTypeDescription = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_fileTypeDescription = castValue;
                    return true;
                }
                return false;
            }
            case "m_fileExtensions":
            case "fileExtensions":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_fileExtensions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
