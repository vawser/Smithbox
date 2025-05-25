// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkPackedVector3Data : HavokData<hkPackedVector3> 
{
    private static readonly System.Reflection.FieldInfo _valuesInfo = typeof(hkPackedVector3).GetField("m_values")!;
    public hkPackedVector3Data(HavokType type, hkPackedVector3 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_values":
            case "values":
            {
                if (instance.m_values is not TGet castValue) return false;
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
            case "m_values":
            case "values":
            {
                if (value is not short[] castValue || castValue.Length != 4) return false;
                try
                {
                    _valuesInfo.SetValue(instance, value);
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
