// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxMesh>
{
    public hkxMeshData(Havok.hk2014.hkxMesh instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2014.hkxMeshSection?> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_userChannelInfos":
            case "userChannelInfos":
            {
                if (value is not List<Havok.hk2014.hkxMesh.UserChannelInfo?> castValue) return false;
                instance.m_userChannelInfos = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
