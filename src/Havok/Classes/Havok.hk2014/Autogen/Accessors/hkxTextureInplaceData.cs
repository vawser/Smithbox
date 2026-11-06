// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxTextureInplaceData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxTextureInplace>
{
    private static readonly System.Reflection.FieldInfo _fileTypeInfo = typeof(Havok.hk2014.hkxTextureInplace).GetField("m_fileType")!;
    public hkxTextureInplaceData(Havok.hk2014.hkxTextureInplace instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fileType":
            case "fileType":
            {
                value = instance.m_fileType;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_fileType":
            case "fileType":
            {
                if (value is not byte[] castValue || castValue.Length != 4) return false;
                try
                {
                    _fileTypeInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_data":
            case "data":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_data = castValue;
                return true;
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
