// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBendLinkConstraintSetLinkData : HavokData<hclBendLinkConstraintSet.Link> 
{
    public hclBendLinkConstraintSetLinkData(HavokType type, hclBendLinkConstraintSet.Link instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_bendMinLength":
            case "bendMinLength":
            {
                if (instance.m_bendMinLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stretchMaxLength":
            case "stretchMaxLength":
            {
                if (instance.m_stretchMaxLength is not TGet castValue) return false;
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
            case "m_stretchStiffness":
            case "stretchStiffness":
            {
                if (instance.m_stretchStiffness is not TGet castValue) return false;
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
            case "m_bendStiffness":
            case "bendStiffness":
            {
                if (value is not float castValue) return false;
                instance.m_bendStiffness = castValue;
                return true;
            }
            case "m_stretchStiffness":
            case "stretchStiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stretchStiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
