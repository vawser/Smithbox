// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPulleyConstraintDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPulleyConstraintData.Atoms>
{
    public hkpPulleyConstraintDataAtomsData(Havok.hk2014.hkpPulleyConstraintData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_translations":
            case "translations":
            {
                value = instance.m_translations;
                return true;
            }
            case "m_pulley":
            case "pulley":
            {
                value = instance.m_pulley;
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
            case "m_translations":
            case "translations":
            {
                if (value is not Havok.hk2014.hkpSetLocalTranslationsConstraintAtom castValue) return false;
                instance.m_translations = castValue;
                return true;
            }
            case "m_pulley":
            case "pulley":
            {
                if (value is not Havok.hk2014.hkpPulleyConstraintAtom castValue) return false;
                instance.m_pulley = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
