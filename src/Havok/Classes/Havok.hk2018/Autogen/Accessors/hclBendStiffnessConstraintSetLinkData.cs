// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBendStiffnessConstraintSetLinkData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBendStiffnessConstraintSet.Link>
{
    public hclBendStiffnessConstraintSetLinkData(Havok.hk2018.hclBendStiffnessConstraintSet.Link instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weightA":
            case "weightA":
            {
                value = instance.m_weightA;
                return true;
            }
            case "m_weightB":
            case "weightB":
            {
                value = instance.m_weightB;
                return true;
            }
            case "m_weightC":
            case "weightC":
            {
                value = instance.m_weightC;
                return true;
            }
            case "m_weightD":
            case "weightD":
            {
                value = instance.m_weightD;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                value = instance.m_bendStiffness;
                return true;
            }
            case "m_restCurvature":
            case "restCurvature":
            {
                value = instance.m_restCurvature;
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
            case "m_particleC":
            case "particleC":
            {
                value = instance.m_particleC;
                return true;
            }
            case "m_particleD":
            case "particleD":
            {
                value = instance.m_particleD;
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
            case "m_weightA":
            case "weightA":
            {
                if (value is not float castValue) return false;
                instance.m_weightA = castValue;
                return true;
            }
            case "m_weightB":
            case "weightB":
            {
                if (value is not float castValue) return false;
                instance.m_weightB = castValue;
                return true;
            }
            case "m_weightC":
            case "weightC":
            {
                if (value is not float castValue) return false;
                instance.m_weightC = castValue;
                return true;
            }
            case "m_weightD":
            case "weightD":
            {
                if (value is not float castValue) return false;
                instance.m_weightD = castValue;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (value is not float castValue) return false;
                instance.m_bendStiffness = castValue;
                return true;
            }
            case "m_restCurvature":
            case "restCurvature":
            {
                if (value is not float castValue) return false;
                instance.m_restCurvature = castValue;
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
            case "m_particleC":
            case "particleC":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleC = castValue;
                return true;
            }
            case "m_particleD":
            case "particleD":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleD = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
