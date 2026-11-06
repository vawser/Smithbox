// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshEdgeZipperIntervalData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshEdgeZipper.Interval>
{
    public hkaiNavMeshEdgeZipperIntervalData(Havok.hk2019.hkaiNavMeshEdgeZipper.Interval instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startT":
            case "startT":
            {
                value = instance.m_startT;
                return true;
            }
            case "m_endT":
            case "endT":
            {
                value = instance.m_endT;
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
            case "m_startT":
            case "startT":
            {
                if (value is not float castValue) return false;
                instance.m_startT = castValue;
                return true;
            }
            case "m_endT":
            case "endT":
            {
                if (value is not float castValue) return false;
                instance.m_endT = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
