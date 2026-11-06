// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkMpRationalData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkMpRational>
{
    public hkMpRationalData(Havok.hk2019.hkMpRational instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_num":
            case "num":
            {
                value = instance.m_num;
                return true;
            }
            case "m_den":
            case "den":
            {
                value = instance.m_den;
                return true;
            }
            case "m_signed":
            case "signed":
            {
                value = instance.m_signed;
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
            case "m_num":
            case "num":
            {
                if (value is not Havok.hk2019.hkMpUint castValue) return false;
                instance.m_num = castValue;
                return true;
            }
            case "m_den":
            case "den":
            {
                if (value is not Havok.hk2019.hkMpUint castValue) return false;
                instance.m_den = castValue;
                return true;
            }
            case "m_signed":
            case "signed":
            {
                if (value is not bool castValue) return false;
                instance.m_signed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
