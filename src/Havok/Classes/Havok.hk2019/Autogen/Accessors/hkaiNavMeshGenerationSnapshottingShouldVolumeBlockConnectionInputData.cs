// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshGenerationSnapshottingShouldVolumeBlockConnectionInputData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionInput>
{
    public hkaiNavMeshGenerationSnapshottingShouldVolumeBlockConnectionInputData(Havok.hk2019.hkaiNavMeshGeneration.Snapshotting.ShouldVolumeBlockConnectionInput instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_material":
            case "material":
            {
                value = instance.m_material;
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
            case "m_material":
            case "material":
            {
                if (value is not Havok.hk2019.hkHandle<uint> castValue) return false;
                instance.m_material = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
