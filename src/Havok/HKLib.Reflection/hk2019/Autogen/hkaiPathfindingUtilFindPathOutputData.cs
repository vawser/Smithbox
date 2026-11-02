// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiPathfindingUtilFindPathOutputData : HavokData<HKLib.hk2019.hkaiPathfindingUtil.FindPathOutput> 
{
    public hkaiPathfindingUtilFindPathOutputData(HavokType type, HKLib.hk2019.hkaiPathfindingUtil.FindPathOutput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_clearanceCacheMisses":
            case "clearanceCacheMisses":
            {
                if (instance.m_clearanceCacheMisses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clearanceCacheIdentifier":
            case "clearanceCacheIdentifier":
            {
                if (instance.m_clearanceCacheIdentifier is not TGet castValue) return false;
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
                if (value is not List<HKLib.hk2019.hkaiPath.PathPoint> castValue) return false;
                instance.m_pathOut = castValue;
                return true;
            }
            case "m_outputParameters":
            case "outputParameters":
            {
                if (value is not HKLib.hk2019.hkaiAstarOutputParameters castValue) return false;
                instance.m_outputParameters = castValue;
                return true;
            }
            case "m_clearanceCacheMisses":
            case "clearanceCacheMisses":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_clearanceCacheMisses = castValue;
                return true;
            }
            case "m_clearanceCacheIdentifier":
            case "clearanceCacheIdentifier":
            {
                if (value is not HKLib.hk2019.hkHandle<byte> castValue) return false;
                instance.m_clearanceCacheIdentifier = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
