// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpSetLocalTranslationsConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpSetLocalTranslationsConstraintAtom>
{
    public hkpSetLocalTranslationsConstraintAtomData(Havok.hk2015.hkpSetLocalTranslationsConstraintAtom instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_translationA":
            case "translationA":
            {
                value = instance.m_translationA;
                return true;
            }
            case "m_translationB":
            case "translationB":
            {
                value = instance.m_translationB;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2015.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2015.hkpConstraintAtom.AtomType)ushortValue;
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
