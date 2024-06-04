// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxMeshData : HavokData<hkxMesh> 
{
    public hkxMeshData(HavokType type, hkxMesh instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<hkxMeshSection?> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_userChannelInfos":
            case "userChannelInfos":
            {
                if (value is not List<hkxMesh.UserChannelInfo?> castValue) return false;
                instance.m_userChannelInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
