// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxTextureFileData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxTextureFile>
{
    public hkxTextureFileData(Havok.hk55.hkxTextureFile instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
