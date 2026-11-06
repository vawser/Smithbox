// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPoweredChainDataData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPoweredChainData>
{
    public hkpPoweredChainDataData(Havok.hk2014.hkpPoweredChainData instance) : base(instance) { }

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
            case "m_cfmLinAdd":
            case "cfmLinAdd":
            {
                value = instance.m_cfmLinAdd;
                return true;
            }
            case "m_cfmLinMul":
            case "cfmLinMul":
            {
                value = instance.m_cfmLinMul;
                return true;
            }
            case "m_cfmAngAdd":
            case "cfmAngAdd":
            {
                value = instance.m_cfmAngAdd;
                return true;
            }
            case "m_cfmAngMul":
            case "cfmAngMul":
            {
                value = instance.m_cfmAngMul;
                return true;
            }
            case "m_maxErrorDistance":
            case "maxErrorDistance":
            {
                value = instance.m_maxErrorDistance;
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
                if (value is not List<Havok.hk2014.hkpPoweredChainDataConstraintInfo> castValue) return false;
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
            case "m_cfmLinAdd":
            case "cfmLinAdd":
            {
                if (value is not float castValue) return false;
                instance.m_cfmLinAdd = castValue;
                return true;
            }
            case "m_cfmLinMul":
            case "cfmLinMul":
            {
                if (value is not float castValue) return false;
                instance.m_cfmLinMul = castValue;
                return true;
            }
            case "m_cfmAngAdd":
            case "cfmAngAdd":
            {
                if (value is not float castValue) return false;
                instance.m_cfmAngAdd = castValue;
                return true;
            }
            case "m_cfmAngMul":
            case "cfmAngMul":
            {
                if (value is not float castValue) return false;
                instance.m_cfmAngMul = castValue;
                return true;
            }
            case "m_maxErrorDistance":
            case "maxErrorDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxErrorDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
