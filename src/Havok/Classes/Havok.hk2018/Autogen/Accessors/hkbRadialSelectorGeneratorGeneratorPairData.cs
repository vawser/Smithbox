// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbRadialSelectorGeneratorGeneratorPairData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbRadialSelectorGenerator.GeneratorPair>
{
    private static readonly System.Reflection.FieldInfo _generatorsInfo = typeof(Havok.hk2018.hkbRadialSelectorGenerator.GeneratorPair).GetField("m_generators")!;
    public hkbRadialSelectorGeneratorGeneratorPairData(Havok.hk2018.hkbRadialSelectorGenerator.GeneratorPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generators":
            case "generators":
            {
                value = instance.m_generators;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                value = instance.m_minAngle;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
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
            case "m_generators":
            case "generators":
            {
                if (value is not Havok.hk2018.hkbRadialSelectorGenerator.GeneratorInfo[] castValue || castValue.Length != 2) return false;
                try
                {
                    _generatorsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (value is not float castValue) return false;
                instance.m_minAngle = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
