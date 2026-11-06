// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpBalanceModifierStepInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpBalanceModifierStepInfo>
{
    public hkbpBalanceModifierStepInfoData(Havok.hk2014.hkbpBalanceModifierStepInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
                return true;
            }
            case "m_fractionOfSolution":
            case "fractionOfSolution":
            {
                value = instance.m_fractionOfSolution;
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
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            case "m_fractionOfSolution":
            case "fractionOfSolution":
            {
                if (value is not float castValue) return false;
                instance.m_fractionOfSolution = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
