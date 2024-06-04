// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBendStiffnessConstraintSetLinkData : HavokData<hclBendStiffnessConstraintSet.Link> 
{
    public hclBendStiffnessConstraintSetLinkData(HavokType type, hclBendStiffnessConstraintSet.Link instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_weightA":
            case "weightA":
            {
                if (instance.m_weightA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weightB":
            case "weightB":
            {
                if (instance.m_weightB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weightC":
            case "weightC":
            {
                if (instance.m_weightC is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weightD":
            case "weightD":
            {
                if (instance.m_weightD is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (instance.m_bendStiffness is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_restCurvature":
            case "restCurvature":
            {
                if (instance.m_restCurvature is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                if (instance.m_particleA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                if (instance.m_particleB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleC":
            case "particleC":
            {
                if (instance.m_particleC is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleD":
            case "particleD":
            {
                if (instance.m_particleD is not TGet castValue) return false;
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
