// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkAabbHalfData : HavokData<HKLib.hk2019.hkAabbHalf> 
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(HKLib.hk2019.hkAabbHalf).GetField("m_data")!;
    public hkAabbHalfData(HavokType type, HKLib.hk2019.hkAabbHalf instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_data":
            case "data":
            {
                if (instance.m_data is not TGet castValue) return false;
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
            case "m_data":
            case "data":
            {
                if (value is not ushort[] castValue || castValue.Length != 8) return false;
                try
                {
                    _dataInfo.SetValue(instance, value);
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
