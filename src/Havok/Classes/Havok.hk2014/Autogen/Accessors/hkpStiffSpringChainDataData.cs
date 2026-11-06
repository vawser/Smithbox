// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpStiffSpringChainDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpStiffSpringChainData>
{
    public hkpStiffSpringChainDataData(Havok.hk2014.hkpStiffSpringChainData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                value = instance.m_atoms;
                return true;
            }
            case "m_infos":
            case "infos":
            {
                value = instance.m_infos;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                value = instance.m_tau;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                value = instance.m_damping;
                return true;
            }
            case "m_cfm":
            case "cfm":
            {
                value = instance.m_cfm;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (value is not Havok.hk2014.hkpBridgeAtoms castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            case "m_infos":
            case "infos":
            {
                if (value is not List<Havok.hk2014.hkpStiffSpringChainDataConstraintInfo> castValue) return false;
                instance.m_infos = castValue;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                if (value is not float castValue) return false;
                instance.m_tau = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_cfm":
            case "cfm":
            {
                if (value is not float castValue) return false;
                instance.m_cfm = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
