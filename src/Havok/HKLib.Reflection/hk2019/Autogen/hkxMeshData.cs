// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMeshData : HavokData<HKLib.hk2019.hkxMesh> 
{
    public hkxMeshData(HavokType type, HKLib.hk2019.hkxMesh instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sections":
            case "sections":
            {
                if (instance.m_sections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userChannelInfos":
            case "userChannelInfos":
            {
                if (instance.m_userChannelInfos is not TGet castValue) return false;
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
            case "m_sections":
            case "sections":
            {
                if (value is not List<HKLib.hk2019.hkxMeshSection?> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_userChannelInfos":
            case "userChannelInfos":
            {
                if (value is not List<HKLib.hk2019.hkxMesh.UserChannelInfo?> castValue) return false;
                instance.m_userChannelInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
