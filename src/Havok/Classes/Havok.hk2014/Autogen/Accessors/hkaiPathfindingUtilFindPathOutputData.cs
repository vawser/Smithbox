// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiPathfindingUtilFindPathOutputData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiPathfindingUtil.FindPathOutput>
{
    public hkaiPathfindingUtilFindPathOutputData(Havok.hk2014.hkaiPathfindingUtil.FindPathOutput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
                if (value is not List<Havok.hk2014.hkaiPath.PathPoint> castValue) return false;
                instance.m_pathOut = castValue;
                return true;
            }
            case "m_outputParameters":
            case "outputParameters":
            {
                if (value is not Havok.hk2014.hkaiAstarOutputParameters castValue) return false;
                instance.m_outputParameters = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
