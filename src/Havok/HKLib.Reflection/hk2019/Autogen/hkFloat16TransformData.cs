// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkFloat16TransformData : HavokData<HKLib.hk2019.hkFloat16Transform> 
{
    private static readonly System.Reflection.FieldInfo _elementsInfo = typeof(HKLib.hk2019.hkFloat16Transform).GetField("m_elements")!;
    public hkFloat16TransformData(HavokType type, HKLib.hk2019.hkFloat16Transform instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                if (instance.m_elements is not TGet castValue) return false;
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
            case "m_elements":
            case "elements":
            {
                if (value is not HKLib.hk2019.hkFloat16[] castValue || castValue.Length != 12) return false;
                try
                {
                    _elementsInfo.SetValue(instance, value);
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
