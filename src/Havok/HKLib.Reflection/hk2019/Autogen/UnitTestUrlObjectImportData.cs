// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestUrlObjectImportData : HavokData<HKLib.hk2019.UnitTest.UrlObject.Import> 
{
    public UnitTestUrlObjectImportData(HavokType type, HKLib.hk2019.UnitTest.UrlObject.Import instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_strong":
            case "strong":
            {
                if (instance.m_strong is not TGet castValue) return false;
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
            case "m_id":
            case "id":
            {
                if (value is not int castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_strong":
            case "strong":
            {
                if (value is not bool castValue) return false;
                instance.m_strong = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
