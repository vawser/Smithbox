// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkFpMathDetailMultiWordUintData : HavokData<HKLib.hk2019.hkFpMath.Detail.MultiWordUint> 
{
    private static readonly System.Reflection.FieldInfo _wordsInfo = typeof(HKLib.hk2019.hkFpMath.Detail.MultiWordUint).GetField("m_words")!;
    public hkFpMathDetailMultiWordUintData(HavokType type, HKLib.hk2019.hkFpMath.Detail.MultiWordUint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_words":
            case "words":
            {
                if (instance.m_words is not TGet castValue) return false;
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
            case "m_words":
            case "words":
            {
                if (value is not ulong[] castValue || castValue.Length != 3) return false;
                try
                {
                    _wordsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
