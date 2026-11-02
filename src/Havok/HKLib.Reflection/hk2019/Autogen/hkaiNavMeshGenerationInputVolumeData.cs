// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavMeshGenerationInputVolumeData : HavokData<HKLib.hk2019.hkaiNavMeshGeneration.InputVolume> 
{
    public hkaiNavMeshGenerationInputVolumeData(HavokType type, HKLib.hk2019.hkaiNavMeshGeneration.InputVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faces":
            case "faces":
            {
                if (instance.m_faces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
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
            case "m_faces":
            case "faces":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.InputVolume.Face> castValue) return false;
                instance.m_faces = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<HKLib.hk2019.hkaiNavMeshGeneration.InputVolume.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<HKLib.hk2019.hkFpMath.FpIntVec3_4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
