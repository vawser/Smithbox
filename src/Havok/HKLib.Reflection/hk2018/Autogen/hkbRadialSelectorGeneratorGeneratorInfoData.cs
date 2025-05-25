// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRadialSelectorGeneratorGeneratorInfoData : HavokData<hkbRadialSelectorGenerator.GeneratorInfo> 
{
    public hkbRadialSelectorGeneratorGeneratorInfoData(HavokType type, hkbRadialSelectorGenerator.GeneratorInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generator":
            case "generator":
            {
                if (instance.m_generator is null)
                {
                    return true;
                }
                if (instance.m_generator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_angle":
            case "angle":
            {
                if (instance.m_angle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radialSpeed":
            case "radialSpeed":
            {
                if (instance.m_radialSpeed is not TGet castValue) return false;
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
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is hkbGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_angle":
            case "angle":
            {
                if (value is not float castValue) return false;
                instance.m_angle = castValue;
                return true;
            }
            case "m_radialSpeed":
            case "radialSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_radialSpeed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
