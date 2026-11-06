// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshEdgeZipperZipEdgesSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshEdgeZipper.ZipEdgesSettings>
{
    public hkaiNavMeshEdgeZipperZipEdgesSettingsData(Havok.hk2019.hkaiNavMeshEdgeZipper.ZipEdgesSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgesSorted":
            case "edgesSorted":
            {
                value = instance.m_edgesSorted;
                return true;
            }
            case "m_clamp":
            case "clamp":
            {
                value = instance.m_clamp;
                return true;
            }
            case "m_tolerance":
            case "tolerance":
            {
                value = instance.m_tolerance;
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
            case "m_edgesSorted":
            case "edgesSorted":
            {
                if (value is not bool castValue) return false;
                instance.m_edgesSorted = castValue;
                return true;
            }
            case "m_clamp":
            case "clamp":
            {
                if (value is not bool castValue) return false;
                instance.m_clamp = castValue;
                return true;
            }
            case "m_tolerance":
            case "tolerance":
            {
                if (value is not float castValue) return false;
                instance.m_tolerance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
