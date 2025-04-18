// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterStringDataFileNameMeshNamePairData : HavokData<hkbCharacterStringData.FileNameMeshNamePair> 
{
    public hkbCharacterStringDataFileNameMeshNamePairData(HavokType type, hkbCharacterStringData.FileNameMeshNamePair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fileName":
            case "fileName":
            {
                if (instance.m_fileName is null)
                {
                    return true;
                }
                if (instance.m_fileName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_meshName":
            case "meshName":
            {
                if (instance.m_meshName is null)
                {
                    return true;
                }
                if (instance.m_meshName is TGet castValue)
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
