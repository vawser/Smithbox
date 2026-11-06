// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxTextureInplaceData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxTextureInplace>
{
    private static readonly System.Reflection.FieldInfo _fileTypeInfo = typeof(Havok.hk55.hkxTextureInplace).GetField("m_fileType")!;
    public hkxTextureInplaceData(Havok.hk55.hkxTextureInplace instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
