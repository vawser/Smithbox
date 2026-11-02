// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestCustomFormatterData : HavokData<HKLib.hk2019.UnitTest.CustomFormatter> 
{
    public UnitTestCustomFormatterData(HavokType type, HKLib.hk2019.UnitTest.CustomFormatter instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_buf":
            case "buf":
            {
                if (instance.m_buf is null)
                {
                    return true;
                }
                if (instance.m_buf is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_buf":
            case "buf":
            {
                if (value is null)
                {
                    instance.m_buf = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_buf = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
