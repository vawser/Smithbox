// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class FSHKPlusfsaAclCompressedAnimationanimationCompressionParamsData : HavokData<HKLib.hk2019.FSHKPlus.fsaAclCompressedAnimation.animationCompressionParams> 
{
    public FSHKPlusfsaAclCompressedAnimationanimationCompressionParamsData(HavokType type, HKLib.hk2019.FSHKPlus.fsaAclCompressedAnimation.animationCompressionParams instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pSettings":
            case "pSettings":
            {
                if (instance.m_pSettings is null)
                {
                    return true;
                }
                if (instance.m_pSettings is TGet castValue)
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
            case "m_pSettings":
            case "pSettings":
            {
                if (value is null)
                {
                    instance.m_pSettings = default;
                    return true;
                }
                if (value is HKLib.hk2019.FSHKPlus.FSA_ACL_COMPRESSION_SETTINGS castValue)
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
