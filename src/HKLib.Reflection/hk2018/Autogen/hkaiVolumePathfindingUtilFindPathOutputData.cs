// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiVolumePathfindingUtil;

namespace HKLib.Reflection.hk2018;

internal class hkaiVolumePathfindingUtilFindPathOutputData : HavokData<FindPathOutput> 
{
    public hkaiVolumePathfindingUtilFindPathOutputData(HavokType type, FindPathOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_visitedCells":
            case "visitedCells":
            {
                if (instance.m_visitedCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_visitedEdges":
            case "visitedEdges":
            {
                if (instance.m_visitedEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathOut":
            case "pathOut":
            {
                if (instance.m_pathOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputParameters":
            case "outputParameters":
            {
                if (instance.m_outputParameters is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_visitedCells":
            case "visitedCells":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_visitedCells = castValue;
                return true;
            }
            case "m_visitedEdges":
            case "visitedEdges":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_visitedEdges = castValue;
                return true;
            }
            case "m_pathOut":
            case "pathOut":
            {
                if (value is not List<hkaiPath.PathPoint> castValue) return false;
                instance.m_pathOut = castValue;
                return true;
            }
            case "m_outputParameters":
            case "outputParameters":
            {
                if (value is not hkaiAstarOutputParameters castValue) return false;
                instance.m_outputParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
