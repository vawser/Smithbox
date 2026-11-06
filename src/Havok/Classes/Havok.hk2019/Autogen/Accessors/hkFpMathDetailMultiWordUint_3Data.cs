// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkFpMathDetailMultiWordUint_3Data : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkFpMath.Detail.MultiWordUint_3>
{
    private static readonly System.Reflection.FieldInfo _wordsInfo = typeof(Havok.hk2019.hkFpMath.Detail.MultiWordUint_3).GetField("m_words")!;
    public hkFpMathDetailMultiWordUint_3Data(Havok.hk2019.hkFpMath.Detail.MultiWordUint_3 instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_words":
            case "words":
            {
                value = instance.m_words;
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
            case "m_words":
            case "words":
            {
                if (value is not ulong[] castValue || castValue.Length != 2) return false;
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
