// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hk;

namespace HKLib.Reflection.hk2018;

internal class hkFileDialogFilterData : HavokData<FileDialogFilter> 
{
    public hkFileDialogFilterData(HavokType type, FileDialogFilter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fileTypeDescription":
            case "fileTypeDescription":
            {
                if (instance.m_fileTypeDescription is null)
                {
                    return true;
                }
                if (instance.m_fileTypeDescription is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_fileExtensions":
            case "fileExtensions":
            {
                if (instance.m_fileExtensions is not TGet castValue) return false;
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
