// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshUtilsResolveEdgePenetrationsInputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshUtils.ResolveEdgePenetrationsInput>
{
    public hkaiNavMeshUtilsResolveEdgePenetrationsInputData(Havok.hk2018.hkaiNavMeshUtils.ResolveEdgePenetrationsInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_queryPt":
            case "queryPt":
            {
                value = instance.m_queryPt;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                value = instance.m_radius;
                return true;
            }
            case "m_maxVerticalDistance":
            case "maxVerticalDistance":
            {
                value = instance.m_maxVerticalDistance;
                return true;
            }
            case "m_initialSearchRadius":
            case "initialSearchRadius":
            {
                value = instance.m_initialSearchRadius;
                return true;
            }
            case "m_maxNumIterations":
            case "maxNumIterations":
            {
                value = instance.m_maxNumIterations;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                value = instance.m_edgeFilter;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                value = instance.m_filterInfo;
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
            case "m_queryPt":
            case "queryPt":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_queryPt = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_maxVerticalDistance":
            case "maxVerticalDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxVerticalDistance = castValue;
                return true;
            }
            case "m_initialSearchRadius":
            case "initialSearchRadius":
            {
                if (value is not float castValue) return false;
                instance.m_initialSearchRadius = castValue;
                return true;
            }
            case "m_maxNumIterations":
            case "maxNumIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumIterations = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (value is null)
                {
                    instance.m_edgeFilter = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiAstarEdgeFilter castValue)
                {
                    instance.m_edgeFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
