// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavMeshEdgeZipperZippedIntervalData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavMeshEdgeZipper.ZippedInterval>
{
    public hkaiNavMeshEdgeZipperZippedIntervalData(Havok.hk2018.hkaiNavMeshEdgeZipper.ZippedInterval instance) : base(instance) { }

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
            case "m_originalIdx":
            case "originalIdx":
            {
                value = instance.m_originalIdx;
                return true;
            }
            case "m_oppositeZippedIntervalIdx":
            case "oppositeZippedIntervalIdx":
            {
                value = instance.m_oppositeZippedIntervalIdx;
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
            case "m_originalIdx":
            case "originalIdx":
            {
                if (value is not int castValue) return false;
                instance.m_originalIdx = castValue;
                return true;
            }
            case "m_oppositeZippedIntervalIdx":
            case "oppositeZippedIntervalIdx":
            {
                if (value is not int castValue) return false;
                instance.m_oppositeZippedIntervalIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
