// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclBendLinkConstraintSetMxSingleData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclBendLinkConstraintSetMx.Single>
{
    public hclBendLinkConstraintSetMxSingleData(Havok.hk2014.hclBendLinkConstraintSetMx.Single instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bendMinLength":
            case "bendMinLength":
            {
                value = instance.m_bendMinLength;
                return true;
            }
            case "m_stretchMaxLength":
            case "stretchMaxLength":
            {
                value = instance.m_stretchMaxLength;
                return true;
            }
            case "m_stretchStiffness":
            case "stretchStiffness":
            {
                value = instance.m_stretchStiffness;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                value = instance.m_bendStiffness;
                return true;
            }
            case "m_invMassA":
            case "invMassA":
            {
                value = instance.m_invMassA;
                return true;
            }
            case "m_invMassB":
            case "invMassB":
            {
                value = instance.m_invMassB;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                value = instance.m_particleA;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                value = instance.m_particleB;
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
            case "m_bendMinLength":
            case "bendMinLength":
            {
                if (value is not float castValue) return false;
                instance.m_bendMinLength = castValue;
                return true;
            }
            case "m_stretchMaxLength":
            case "stretchMaxLength":
            {
                if (value is not float castValue) return false;
                instance.m_stretchMaxLength = castValue;
                return true;
            }
            case "m_stretchStiffness":
            case "stretchStiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stretchStiffness = castValue;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (value is not float castValue) return false;
                instance.m_bendStiffness = castValue;
                return true;
            }
            case "m_invMassA":
            case "invMassA":
            {
                if (value is not float castValue) return false;
                instance.m_invMassA = castValue;
                return true;
            }
            case "m_invMassB":
            case "invMassB":
            {
                if (value is not float castValue) return false;
                instance.m_invMassB = castValue;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleA = castValue;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
