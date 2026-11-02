// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkAabbUint32Data : HavokData<HKLib.hk2019.hkAabbUint32> 
{
    private static readonly System.Reflection.FieldInfo _minInfo = typeof(HKLib.hk2019.hkAabbUint32).GetField("m_min")!;
    private static readonly System.Reflection.FieldInfo _expansionMinInfo = typeof(HKLib.hk2019.hkAabbUint32).GetField("m_expansionMin")!;
    private static readonly System.Reflection.FieldInfo _maxInfo = typeof(HKLib.hk2019.hkAabbUint32).GetField("m_max")!;
    private static readonly System.Reflection.FieldInfo _expansionMaxInfo = typeof(HKLib.hk2019.hkAabbUint32).GetField("m_expansionMax")!;
    public hkAabbUint32Data(HavokType type, HKLib.hk2019.hkAabbUint32 instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                if (instance.m_min is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_expansionMin":
            case "expansionMin":
            {
                if (instance.m_expansionMin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_expansionShift":
            case "expansionShift":
            {
                if (instance.m_expansionShift is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (instance.m_max is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_expansionMax":
            case "expansionMax":
            {
                if (instance.m_expansionMax is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shapeKeyByte":
            case "shapeKeyByte":
            {
                if (instance.m_shapeKeyByte is not TGet castValue) return false;
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
            case "m_min":
            case "min":
            {
                if (value is not uint[] castValue || castValue.Length != 3) return false;
                try
                {
                    _minInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_expansionMin":
            case "expansionMin":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _expansionMinInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_expansionShift":
            case "expansionShift":
            {
                if (value is not byte castValue) return false;
                instance.m_expansionShift = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not uint[] castValue || castValue.Length != 3) return false;
                try
                {
                    _maxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_expansionMax":
            case "expansionMax":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _expansionMaxInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_shapeKeyByte":
            case "shapeKeyByte":
            {
                if (value is not byte castValue) return false;
                instance.m_shapeKeyByte = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
