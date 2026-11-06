// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiVolumePathfindingUtilFindPathOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiVolumePathfindingUtil.FindPathOutput>
{
    public hkaiVolumePathfindingUtilFindPathOutputData(Havok.hk2018.hkaiVolumePathfindingUtil.FindPathOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_visitedCells":
            case "visitedCells":
            {
                value = instance.m_visitedCells;
                return true;
            }
            case "m_visitedEdges":
            case "visitedEdges":
            {
                value = instance.m_visitedEdges;
                return true;
            }
            case "m_pathOut":
            case "pathOut":
            {
                value = instance.m_pathOut;
                return true;
            }
            case "m_outputParameters":
            case "outputParameters":
            {
                value = instance.m_outputParameters;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is not List<Havok.hk2018.hkaiPath.PathPoint> castValue) return false;
                instance.m_pathOut = castValue;
                return true;
            }
            case "m_outputParameters":
            case "outputParameters":
            {
                if (value is not Havok.hk2018.hkaiAstarOutputParameters castValue) return false;
                instance.m_outputParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
