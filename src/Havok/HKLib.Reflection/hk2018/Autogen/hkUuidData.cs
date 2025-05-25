// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkUuidData : HavokData<hkUuid> 
{
    private static readonly System.Reflection.FieldInfo _dataInfo = typeof(hkUuid).GetField("m_data")!;
    public hkUuidData(HavokType type, hkUuid instance) : base(type, instance) {}

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
                if (value is not uint[] castValue || castValue.Length != 4) return false;
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
