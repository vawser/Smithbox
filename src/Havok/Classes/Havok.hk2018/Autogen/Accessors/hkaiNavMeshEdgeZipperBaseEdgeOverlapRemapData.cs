// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshEdgeZipperBaseEdgeOverlapRemapData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshEdgeZipper.BaseEdgeOverlapRemap>
{
    public hkaiNavMeshEdgeZipperBaseEdgeOverlapRemapData(Havok.hk2018.hkaiNavMeshEdgeZipper.BaseEdgeOverlapRemap instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_start":
            case "start":
            {
                value = instance.m_start;
                return true;
            }
            case "m_end":
            case "end":
            {
                value = instance.m_end;
                return true;
            }
            case "m_overlapRemap":
            case "overlapRemap":
            {
                value = instance.m_overlapRemap;
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
            case "m_start":
            case "start":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            case "m_overlapRemap":
            case "overlapRemap":
            {
                if (value is not Havok.hk2018.hkaiAffineMap1D castValue) return false;
                instance.m_overlapRemap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
