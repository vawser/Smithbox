// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterStringDataFileNameMeshNamePairData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterStringData.FileNameMeshNamePair>
{
    public hkbCharacterStringDataFileNameMeshNamePairData(Havok.hk2014.hkbCharacterStringData.FileNameMeshNamePair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fileName":
            case "fileName":
            {
                value = instance.m_fileName;
                return true;
            }
            case "m_meshName":
            case "meshName":
            {
                value = instance.m_meshName;
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
            case "m_fileName":
            case "fileName":
            {
                if (value is null)
                {
                    instance.m_fileName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_fileName = castValue;
                    return true;
                }
                return false;
            }
            case "m_meshName":
            case "meshName":
            {
                if (value is null)
                {
                    instance.m_meshName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_meshName = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
