// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkcdStaticMeshTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdStaticMeshTreeConnectivityData : HavokData<Connectivity> 
{
    public hkcdStaticMeshTreeConnectivityData(HavokType type, Connectivity instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_headers":
            case "headers":
            {
                if (instance.m_headers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localLinks":
            case "localLinks":
            {
                if (instance.m_localLinks is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_globalLinks":
            case "globalLinks":
            {
                if (instance.m_globalLinks is not TGet castValue) return false;
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
            case "m_headers":
            case "headers":
            {
                if (value is not List<Connectivity.SectionHeader> castValue) return false;
                instance.m_headers = castValue;
                return true;
            }
            case "m_localLinks":
            case "localLinks":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_localLinks = castValue;
                return true;
            }
            case "m_globalLinks":
            case "globalLinks":
            {
                if (value is not List<hkHandle<uint>> castValue) return false;
                instance.m_globalLinks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
