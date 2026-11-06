// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationInputAreaData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.InputArea>
{
    public hkaiNavMeshGenerationInputAreaData(Havok.hk2019.hkaiNavMeshGeneration.InputArea instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
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
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Havok.hk2019.hkFpMath.FpIntVec2_5> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
