// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationInputVolumeEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.InputVolume.Edge>
{
    public hkaiNavMeshGenerationInputVolumeEdgeData(Havok.hk2019.hkaiNavMeshGeneration.InputVolume.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndex":
            case "faceIndex":
            {
                value = instance.m_faceIndex;
                return true;
            }
            case "m_vertexIndex":
            case "vertexIndex":
            {
                value = instance.m_vertexIndex;
                return true;
            }
            case "m_prevEdgeIndex":
            case "prevEdgeIndex":
            {
                value = instance.m_prevEdgeIndex;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                value = instance.m_nextEdgeIndex;
                return true;
            }
            case "m_oppositeEdgeIndex":
            case "oppositeEdgeIndex":
            {
                value = instance.m_oppositeEdgeIndex;
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
