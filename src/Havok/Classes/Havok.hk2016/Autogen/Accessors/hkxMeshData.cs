// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkxMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkxMesh>
{
    public hkxMeshData(Havok.hk2016.hkxMesh instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
                return true;
            }
            case "m_userChannelInfos":
            case "userChannelInfos":
            {
                value = instance.m_userChannelInfos;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2016.hkxMeshSection?> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_userChannelInfos":
            case "userChannelInfos":
            {
                if (value is not List<Havok.hk2016.hkxMesh.UserChannelInfo?> castValue) return false;
                instance.m_userChannelInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
