// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdStaticMeshTreeConnectivityData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdStaticMeshTree.Connectivity>
{
    public hkcdStaticMeshTreeConnectivityData(Havok.hk2018.hkcdStaticMeshTree.Connectivity instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_headers":
            case "headers":
            {
                value = instance.m_headers;
                return true;
            }
            case "m_localLinks":
            case "localLinks":
            {
                value = instance.m_localLinks;
                return true;
            }
            case "m_globalLinks":
            case "globalLinks":
            {
                value = instance.m_globalLinks;
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
            case "m_headers":
            case "headers":
            {
                if (value is not List<Havok.hk2018.hkcdStaticMeshTree.Connectivity.SectionHeader> castValue) return false;
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
                if (value is not List<Havok.hk2018.hkHandle<uint>> castValue) return false;
                instance.m_globalLinks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
