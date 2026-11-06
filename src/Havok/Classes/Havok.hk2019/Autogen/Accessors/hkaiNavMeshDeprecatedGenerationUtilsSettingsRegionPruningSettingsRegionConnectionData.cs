// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshDeprecatedGenerationUtilsSettingsRegionPruningSettingsRegionConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.RegionPruningSettings.RegionConnection>
{
    public hkaiNavMeshDeprecatedGenerationUtilsSettingsRegionPruningSettingsRegionConnectionData(Havok.hk2019.hkaiNavMeshDeprecatedGenerationUtilsSettings.RegionPruningSettings.RegionConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_a":
            case "a":
            {
                value = instance.m_a;
                return true;
            }
            case "m_b":
            case "b":
            {
                value = instance.m_b;
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
            case "m_a":
            case "a":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_a = castValue;
                return true;
            }
            case "m_b":
            case "b":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_b = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
