// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRadialSelectorGeneratorGeneratorInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRadialSelectorGenerator.GeneratorInfo>
{
    public hkbRadialSelectorGeneratorGeneratorInfoData(Havok.hk2018.hkbRadialSelectorGenerator.GeneratorInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_angle":
            case "angle":
            {
                value = instance.m_angle;
                return true;
            }
            case "m_radialSpeed":
            case "radialSpeed":
            {
                value = instance.m_radialSpeed;
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
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbGenerator castValue)
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
