// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationInputVolumeEdgeData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.InputVolume.Edge> 
{
    public hkaiNavMeshGenerationInputVolumeEdgeData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.InputVolume.Edge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndex":
            case "faceIndex":
            {
                if (instance.m_faceIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexIndex":
            case "vertexIndex":
            {
                if (instance.m_vertexIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_prevEdgeIndex":
            case "prevEdgeIndex":
            {
                if (instance.m_prevEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                if (instance.m_nextEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oppositeEdgeIndex":
            case "oppositeEdgeIndex":
            {
                if (instance.m_oppositeEdgeIndex is not TGet castValue) return false;
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
            case "m_faceIndex":
            case "faceIndex":
            {
                if (value is not int castValue) return false;
                instance.m_faceIndex = castValue;
                return true;
            }
            case "m_vertexIndex":
            case "vertexIndex":
            {
                if (value is not int castValue) return false;
                instance.m_vertexIndex = castValue;
                return true;
            }
            case "m_prevEdgeIndex":
            case "prevEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_prevEdgeIndex = castValue;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_nextEdgeIndex = castValue;
                return true;
            }
            case "m_oppositeEdgeIndex":
            case "oppositeEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_oppositeEdgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
