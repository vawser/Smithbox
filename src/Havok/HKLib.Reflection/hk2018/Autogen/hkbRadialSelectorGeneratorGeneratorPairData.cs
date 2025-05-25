// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbRadialSelectorGeneratorGeneratorPairData : HavokData<hkbRadialSelectorGenerator.GeneratorPair> 
{
    private static readonly System.Reflection.FieldInfo _generatorsInfo = typeof(hkbRadialSelectorGenerator.GeneratorPair).GetField("m_generators")!;
    public hkbRadialSelectorGeneratorGeneratorPairData(HavokType type, hkbRadialSelectorGenerator.GeneratorPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_generators":
            case "generators":
            {
                if (instance.m_generators is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (instance.m_minAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (instance.m_maxAngle is not TGet castValue) return false;
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
            case "m_generators":
            case "generators":
            {
                if (value is not hkbRadialSelectorGenerator.GeneratorInfo[] castValue || castValue.Length != 2) return false;
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
