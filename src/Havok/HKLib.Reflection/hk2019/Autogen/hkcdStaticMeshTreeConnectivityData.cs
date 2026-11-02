// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdStaticMeshTreeConnectivityData : HavokData<HKLib.hk2019.hkcdStaticMeshTree.Connectivity> 
{
    public hkcdStaticMeshTreeConnectivityData(HavokType type, HKLib.hk2019.hkcdStaticMeshTree.Connectivity instance) : base(type, instance) {}

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
                if (value is not List<HKLib.hk2019.hkcdStaticMeshTree.Connectivity.SectionHeader> castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkHandle<uint>> castValue) return false;
                instance.m_globalLinks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
