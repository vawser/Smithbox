// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpPulleyConstraintDataAtomsData : HavokData<hkpPulleyConstraintData.Atoms> 
{
    public hkpPulleyConstraintDataAtomsData(HavokType type, hkpPulleyConstraintData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_translations":
            case "translations":
            {
                if (instance.m_translations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pulley":
            case "pulley":
            {
                if (instance.m_pulley is not TGet castValue) return false;
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
            case "m_translations":
            case "translations":
            {
                if (value is not hkpSetLocalTranslationsConstraintAtom castValue) return false;
                instance.m_translations = castValue;
                return true;
            }
            case "m_pulley":
            case "pulley":
            {
                if (value is not hkpPulleyConstraintAtom castValue) return false;
                instance.m_pulley = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
