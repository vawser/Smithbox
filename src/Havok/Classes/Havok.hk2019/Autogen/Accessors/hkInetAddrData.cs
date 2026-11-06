// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkInetAddrData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkInetAddr>
{
    public hkInetAddrData(Havok.hk2019.hkInetAddr instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ipAddress":
            case "ipAddress":
            {
                value = instance.m_ipAddress;
                return true;
            }
            case "m_port":
            case "port":
            {
                value = instance.m_port;
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
            case "m_ipAddress":
            case "ipAddress":
            {
                if (value is not int castValue) return false;
                instance.m_ipAddress = castValue;
                return true;
            }
            case "m_port":
            case "port":
            {
                if (value is not ushort castValue) return false;
                instance.m_port = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
