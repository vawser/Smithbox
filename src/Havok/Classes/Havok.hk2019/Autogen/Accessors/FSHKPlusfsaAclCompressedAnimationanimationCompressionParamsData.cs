// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class FSHKPlusfsaAclCompressedAnimationanimationCompressionParamsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.FSHKPlus.fsaAclCompressedAnimation.animationCompressionParams>
{
    public FSHKPlusfsaAclCompressedAnimationanimationCompressionParamsData(Havok.hk2019.FSHKPlus.fsaAclCompressedAnimation.animationCompressionParams instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pSettings":
            case "pSettings":
            {
                value = instance.m_pSettings;
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
            case "m_pSettings":
            case "pSettings":
            {
                if (value is null)
                {
                    instance.m_pSettings = default;
                    return true;
                }
                if (value is Havok.hk2019.FSHKPlus.FSA_ACL_COMPRESSION_SETTINGS castValue)
                {
                    instance.m_pSettings = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
