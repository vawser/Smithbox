// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavMeshEdgeZipper;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshEdgeZipperZippedIntervalData : HavokData<ZippedInterval> 
{
    public hkaiNavMeshEdgeZipperZippedIntervalData(HavokType type, ZippedInterval instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startT":
            case "startT":
            {
                if (instance.m_startT is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endT":
            case "endT":
            {
                if (instance.m_endT is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originalIdx":
            case "originalIdx":
            {
                if (instance.m_originalIdx is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_oppositeZippedIntervalIdx":
            case "oppositeZippedIntervalIdx":
            {
                if (instance.m_oppositeZippedIntervalIdx is not TGet castValue) return false;
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
