// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class UnitTestCustomFormatterData : Havok.Formats.HavokFieldAccess<Havok.hk2019.UnitTest.CustomFormatter>
{
    public UnitTestCustomFormatterData(Havok.hk2019.UnitTest.CustomFormatter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_buf":
            case "buf":
            {
                value = instance.m_buf;
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
