// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxTextureInplaceData : HavokData<HKLib.hk2019.hkxTextureInplace> 
{
    private static readonly System.Reflection.FieldInfo _fileTypeInfo = typeof(HKLib.hk2019.hkxTextureInplace).GetField("m_fileType")!;
    public hkxTextureInplaceData(HavokType type, HKLib.hk2019.hkxTextureInplace instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fileType":
            case "fileType":
            {
                if (instance.m_fileType is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_originalFilename":
            case "originalFilename":
            {
                if (instance.m_originalFilename is null)
                {
                    return true;
                }
                if (instance.m_originalFilename is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
