// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpSetLocalTranslationsConstraintAtomData : HavokData<hkpSetLocalTranslationsConstraintAtom> 
{
    public hkpSetLocalTranslationsConstraintAtomData(HavokType type, hkpSetLocalTranslationsConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_translationA":
            case "translationA":
            {
                if (instance.m_translationA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_translationB":
            case "translationB":
            {
                if (instance.m_translationB is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_translationA":
            case "translationA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translationA = castValue;
                return true;
            }
            case "m_translationB":
            case "translationB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_translationB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
