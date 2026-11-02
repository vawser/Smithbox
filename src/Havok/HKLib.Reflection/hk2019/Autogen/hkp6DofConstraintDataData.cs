// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkp6DofConstraintDataData : HavokData<HKLib.hk2019.hkp6DofConstraintData> 
{
    public hkp6DofConstraintDataData(HavokType type, HKLib.hk2019.hkp6DofConstraintData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (instance.m_numNonDirectlySolvedAtoms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blueprints":
            case "blueprints":
            {
                if (instance.m_blueprints is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (value is not byte castValue) return false;
                instance.m_numNonDirectlySolvedAtoms = castValue;
                return true;
            }
            case "m_blueprints":
            case "blueprints":
            {
                if (value is not HKLib.hk2019.hkp6DofConstraintData.Blueprints castValue) return false;
                instance.m_blueprints = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
