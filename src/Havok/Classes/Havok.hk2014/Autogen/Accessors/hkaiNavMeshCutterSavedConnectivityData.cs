// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiNavMeshCutterSavedConnectivityData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiNavMeshCutterSavedConnectivity>
{
    public hkaiNavMeshCutterSavedConnectivityData(Havok.hk2014.hkaiNavMeshCutterSavedConnectivity instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_storage":
            case "storage":
            {
                value = instance.m_storage;
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
            case "m_storage":
            case "storage":
            {
                if (value is not Havok.hk2014.hkSetUint32 castValue) return false;
                instance.m_storage = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
