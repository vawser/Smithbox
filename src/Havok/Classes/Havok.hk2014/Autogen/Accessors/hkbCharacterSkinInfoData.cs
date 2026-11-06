// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterSkinInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterSkinInfo>
{
    public hkbCharacterSkinInfoData(Havok.hk2014.hkbCharacterSkinInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_deformableSkins":
            case "deformableSkins":
            {
                value = instance.m_deformableSkins;
                return true;
            }
            case "m_rigidSkins":
            case "rigidSkins":
            {
                value = instance.m_rigidSkins;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_deformableSkins":
            case "deformableSkins":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_deformableSkins = castValue;
                return true;
            }
            case "m_rigidSkins":
            case "rigidSkins":
            {
                if (value is not List<ulong> castValue) return false;
                instance.m_rigidSkins = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
