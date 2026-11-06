// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshSimplificationUtilsExtraVertexSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshSimplificationUtils.ExtraVertexSettings>
{
    public hkaiNavMeshSimplificationUtilsExtraVertexSettingsData(Havok.hk2019.hkaiNavMeshSimplificationUtils.ExtraVertexSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexSelectionMethod":
            case "vertexSelectionMethod":
            {
                value = instance.m_vertexSelectionMethod;
                return true;
            }
            case "m_vertexFraction":
            case "vertexFraction":
            {
                value = instance.m_vertexFraction;
                return true;
            }
            case "m_areaFraction":
            case "areaFraction":
            {
                value = instance.m_areaFraction;
                return true;
            }
            case "m_minPartitionArea":
            case "minPartitionArea":
            {
                value = instance.m_minPartitionArea;
                return true;
            }
            case "m_numSmoothingIterations":
            case "numSmoothingIterations":
            {
                value = instance.m_numSmoothingIterations;
                return true;
            }
            case "m_iterationDamping":
            case "iterationDamping":
            {
                value = instance.m_iterationDamping;
                return true;
            }
            case "m_addVerticesOnBoundaryEdges":
            case "addVerticesOnBoundaryEdges":
            {
                value = instance.m_addVerticesOnBoundaryEdges;
                return true;
            }
            case "m_addVerticesOnPartitionBorders":
            case "addVerticesOnPartitionBorders":
            {
                value = instance.m_addVerticesOnPartitionBorders;
                return true;
            }
            case "m_boundaryEdgeSplitLength":
            case "boundaryEdgeSplitLength":
            {
                value = instance.m_boundaryEdgeSplitLength;
                return true;
            }
            case "m_partitionBordersSplitLength":
            case "partitionBordersSplitLength":
            {
                value = instance.m_partitionBordersSplitLength;
                return true;
            }
            case "m_userVertexOnBoundaryTolerance":
            case "userVertexOnBoundaryTolerance":
            {
                value = instance.m_userVertexOnBoundaryTolerance;
                return true;
            }
            case "m_userVertices":
            case "userVertices":
            {
                value = instance.m_userVertices;
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
            case "m_vertexSelectionMethod":
            case "vertexSelectionMethod":
            {
                if (value is Havok.hk2019.hkaiNavMeshSimplificationUtils.ExtraVertexSettings.VertexSelectionMethod castValue)
                {
                    instance.m_vertexSelectionMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_vertexSelectionMethod = (Havok.hk2019.hkaiNavMeshSimplificationUtils.ExtraVertexSettings.VertexSelectionMethod)byteValue;
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
