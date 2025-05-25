// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkInetAddrData : HavokData<hkInetAddr> 
{
    public hkInetAddrData(HavokType type, hkInetAddr instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ipAddress":
            case "ipAddress":
            {
                if (instance.m_ipAddress is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_port":
            case "port":
            {
                if (instance.m_port is not TGet castValue) return false;
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
