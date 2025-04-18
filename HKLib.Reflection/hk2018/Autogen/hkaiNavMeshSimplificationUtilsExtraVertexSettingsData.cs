// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavMeshSimplificationUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshSimplificationUtilsExtraVertexSettingsData : HavokData<ExtraVertexSettings> 
{
    public hkaiNavMeshSimplificationUtilsExtraVertexSettingsData(HavokType type, ExtraVertexSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexSelectionMethod":
            case "vertexSelectionMethod":
            {
                if (instance.m_vertexSelectionMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_vertexSelectionMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexFraction":
            case "vertexFraction":
            {
                if (instance.m_vertexFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_areaFraction":
            case "areaFraction":
            {
                if (instance.m_areaFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minPartitionArea":
            case "minPartitionArea":
            {
                if (instance.m_minPartitionArea is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSmoothingIterations":
            case "numSmoothingIterations":
            {
                if (instance.m_numSmoothingIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_iterationDamping":
            case "iterationDamping":
            {
                if (instance.m_iterationDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_addVerticesOnBoundaryEdges":
            case "addVerticesOnBoundaryEdges":
            {
                if (instance.m_addVerticesOnBoundaryEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_addVerticesOnPartitionBorders":
            case "addVerticesOnPartitionBorders":
            {
                if (instance.m_addVerticesOnPartitionBorders is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundaryEdgeSplitLength":
            case "boundaryEdgeSplitLength":
            {
                if (instance.m_boundaryEdgeSplitLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partitionBordersSplitLength":
            case "partitionBordersSplitLength":
            {
                if (instance.m_partitionBordersSplitLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userVertexOnBoundaryTolerance":
            case "userVertexOnBoundaryTolerance":
            {
                if (instance.m_userVertexOnBoundaryTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userVertices":
            case "userVertices":
            {
                if (instance.m_userVertices is not TGet castValue) return false;
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
            case "m_vertexSelectionMethod":
            case "vertexSelectionMethod":
            {
                if (value is ExtraVertexSettings.VertexSelectionMethod castValue)
                {
                    instance.m_vertexSelectionMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_vertexSelectionMethod = (ExtraVertexSettings.VertexSelectionMethod)byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexFraction":
            case "vertexFraction":
            {
                if (value is not float castValue) return false;
                instance.m_vertexFraction = castValue;
                return true;
            }
            case "m_areaFraction":
            case "areaFraction":
            {
                if (value is not float castValue) return false;
                instance.m_areaFraction = castValue;
                return true;
            }
            case "m_minPartitionArea":
            case "minPartitionArea":
            {
                if (value is not float castValue) return false;
                instance.m_minPartitionArea = castValue;
                return true;
            }
            case "m_numSmoothingIterations":
            case "numSmoothingIterations":
            {
                if (value is not int castValue) return false;
                instance.m_numSmoothingIterations = castValue;
                return true;
            }
            case "m_iterationDamping":
            case "iterationDamping":
            {
                if (value is not float castValue) return false;
                instance.m_iterationDamping = castValue;
                return true;
            }
            case "m_addVerticesOnBoundaryEdges":
            case "addVerticesOnBoundaryEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_addVerticesOnBoundaryEdges = castValue;
                return true;
            }
            case "m_addVerticesOnPartitionBorders":
            case "addVerticesOnPartitionBorders":
            {
                if (value is not bool castValue) return false;
                instance.m_addVerticesOnPartitionBorders = castValue;
                return true;
            }
            case "m_boundaryEdgeSplitLength":
            case "boundaryEdgeSplitLength":
            {
                if (value is not float castValue) return false;
                instance.m_boundaryEdgeSplitLength = castValue;
                return true;
            }
            case "m_partitionBordersSplitLength":
            case "partitionBordersSplitLength":
            {
                if (value is not float castValue) return false;
                instance.m_partitionBordersSplitLength = castValue;
                return true;
            }
            case "m_userVertexOnBoundaryTolerance":
            case "userVertexOnBoundaryTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_userVertexOnBoundaryTolerance = castValue;
                return true;
            }
            case "m_userVertices":
            case "userVertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_userVertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
