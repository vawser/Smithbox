// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiGatePathPathGateData : HavokData<HKLib.hk2019.hkaiGatePath.PathGate> 
{
    public hkaiGatePathPathGateData(HavokType type, HKLib.hk2019.hkaiGatePath.PathGate instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_crossingPoint":
            case "crossingPoint":
            {
                if (instance.m_crossingPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gate":
            case "gate":
            {
                if (instance.m_gate is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundarySegments":
            case "boundarySegments":
            {
                if (instance.m_boundarySegments is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_boundarySegments is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_cellKey":
            case "cellKey":
            {
                if (instance.m_cellKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (instance.m_edgeIndex is not TGet castValue) return false;
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
            case "m_crossingPoint":
            case "crossingPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_crossingPoint = castValue;
                return true;
            }
            case "m_gate":
            case "gate":
            {
                if (value is not HKLib.hk2019.hkaiGatePathUtil.Gate castValue) return false;
                instance.m_gate = castValue;
                return true;
            }
            case "m_boundarySegments":
            case "boundarySegments":
            {
                if (value is HKLib.hk2019.hkaiGatePath.BoundarySegmentBits castValue)
                {
                    instance.m_boundarySegments = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_boundarySegments = (HKLib.hk2019.hkaiGatePath.BoundarySegmentBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_cellKey":
            case "cellKey":
            {
                if (value is not uint castValue) return false;
                instance.m_cellKey = castValue;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_edgeIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
