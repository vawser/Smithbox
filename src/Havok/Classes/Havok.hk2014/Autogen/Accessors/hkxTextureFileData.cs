// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxTextureFileData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxTextureFile>
{
    public hkxTextureFileData(Havok.hk2014.hkxTextureFile instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_filename":
            case "filename":
            {
                value = instance.m_filename;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_originalFilename":
            case "originalFilename":
            {
                value = instance.m_originalFilename;
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
            case "m_filename":
            case "filename":
            {
                if (value is null)
                {
                    instance.m_filename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_filename = castValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_originalFilename":
            case "originalFilename":
            {
                if (value is null)
                {
                    instance.m_originalFilename = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_originalFilename = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
