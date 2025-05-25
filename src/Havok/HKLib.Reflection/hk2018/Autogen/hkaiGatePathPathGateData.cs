// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiGatePathUtil;

namespace HKLib.Reflection.hk2018;

internal class hkaiGatePathPathGateData : HavokData<hkaiGatePath.PathGate> 
{
    public hkaiGatePathPathGateData(HavokType type, hkaiGatePath.PathGate instance) : base(type, instance) {}

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
                if (value is not Gate castValue) return false;
                instance.m_gate = castValue;
                return true;
            }
            case "m_boundarySegments":
            case "boundarySegments":
            {
                if (value is hkaiGatePath.BoundarySegmentBits castValue)
                {
                    instance.m_boundarySegments = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_boundarySegments = (hkaiGatePath.BoundarySegmentBits)byteValue;
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
