// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclCompressibleLinkConstraintSetLinkData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclCompressibleLinkConstraintSet.Link>
{
    public hclCompressibleLinkConstraintSetLinkData(Havok.hk2014.hclCompressibleLinkConstraintSet.Link instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_restLength":
            case "restLength":
            {
                value = instance.m_restLength;
                return true;
            }
            case "m_compressionLength":
            case "compressionLength":
            {
                value = instance.m_compressionLength;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                value = instance.m_stiffness;
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
            case "m_restLength":
            case "restLength":
            {
                if (value is not float castValue) return false;
                instance.m_restLength = castValue;
                return true;
            }
            case "m_compressionLength":
            case "compressionLength":
            {
                if (value is not float castValue) return false;
                instance.m_compressionLength = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
