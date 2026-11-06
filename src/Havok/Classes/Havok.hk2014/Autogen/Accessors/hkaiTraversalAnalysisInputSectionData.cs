// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiTraversalAnalysisInputSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiTraversalAnalysisInputSection>
{
    public hkaiTraversalAnalysisInputSectionData(Havok.hk2014.hkaiTraversalAnalysisInputSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_navMeshInstance":
            case "navMeshInstance":
            {
                value = instance.m_navMeshInstance;
                return true;
            }
            case "m_geometry":
            case "geometry":
            {
                value = instance.m_geometry;
                return true;
            }
            case "m_walkableBitfield":
            case "walkableBitfield":
            {
                value = instance.m_walkableBitfield;
                return true;
            }
            case "m_cuttingBitfield":
            case "cuttingBitfield":
            {
                value = instance.m_cuttingBitfield;
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
            case "m_navMeshInstance":
            case "navMeshInstance":
            {
                if (value is null)
                {
                    instance.m_navMeshInstance = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaiNavMeshInstance castValue)
                {
                    instance.m_navMeshInstance = castValue;
                    return true;
                }
                return false;
            }
            case "m_geometry":
            case "geometry":
            {
                if (value is null)
                {
                    instance.m_geometry = default;
                    return true;
                }
                if (value is Havok.hk2014.hkGeometry castValue)
                {
                    instance.m_geometry = castValue;
                    return true;
                }
                return false;
            }
            case "m_walkableBitfield":
            case "walkableBitfield":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_walkableBitfield = castValue;
                return true;
            }
            case "m_cuttingBitfield":
            case "cuttingBitfield":
            {
                if (value is not Havok.hk2014.hkBitField castValue) return false;
                instance.m_cuttingBitfield = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
