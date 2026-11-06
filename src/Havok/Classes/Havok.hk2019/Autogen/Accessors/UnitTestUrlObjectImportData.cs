// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestUrlObjectImportData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.UrlObject.Import>
{
    public UnitTestUrlObjectImportData(Havok.hk2019.UnitTest.UrlObject.Import instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_strong":
            case "strong":
            {
                value = instance.m_strong;
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
