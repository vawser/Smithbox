// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpRefWorldCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpRefWorldCinfo>
{
    public hknpRefWorldCinfoData(Havok.hk2014.hknpRefWorldCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_info":
            case "info":
            {
                value = instance.m_info;
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
            case "m_info":
            case "info":
            {
                if (value is not Havok.hk2014.hknpWorldCinfo castValue) return false;
                instance.m_info = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
