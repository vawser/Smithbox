// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshModifiedAabbsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshModifiedAabbs>
{
    public hkaiNavMeshModifiedAabbsData(Havok.hk2019.hkaiNavMeshModifiedAabbs instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                value = instance.m_modifiedAabbs;
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
            case "m_modifiedAabbs":
            case "modifiedAabbs":
            {
                if (value is not List<List<Havok.hk2019.hkAabb>> castValue) return false;
                instance.m_modifiedAabbs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
