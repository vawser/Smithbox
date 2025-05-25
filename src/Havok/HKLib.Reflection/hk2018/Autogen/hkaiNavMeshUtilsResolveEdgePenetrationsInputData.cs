// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshUtilsResolveEdgePenetrationsInputData : HavokData<ResolveEdgePenetrationsInput> 
{
    public hkaiNavMeshUtilsResolveEdgePenetrationsInputData(HavokType type, ResolveEdgePenetrationsInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_queryPt":
            case "queryPt":
            {
                if (instance.m_queryPt is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVerticalDistance":
            case "maxVerticalDistance":
            {
                if (instance.m_maxVerticalDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialSearchRadius":
            case "initialSearchRadius":
            {
                if (instance.m_initialSearchRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxNumIterations":
            case "maxNumIterations":
            {
                if (instance.m_maxNumIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (instance.m_edgeFilter is null)
                {
                    return true;
                }
                if (instance.m_edgeFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (instance.m_filterInfo is not TGet castValue) return false;
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
                if (value is hkaiAstarEdgeFilter castValue)
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
