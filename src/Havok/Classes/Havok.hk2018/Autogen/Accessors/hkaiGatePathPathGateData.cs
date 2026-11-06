// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiGatePathPathGateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiGatePath.PathGate>
{
    public hkaiGatePathPathGateData(Havok.hk2018.hkaiGatePath.PathGate instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_crossingPoint":
            case "crossingPoint":
            {
                value = instance.m_crossingPoint;
                return true;
            }
            case "m_gate":
            case "gate":
            {
                value = instance.m_gate;
                return true;
            }
            case "m_boundarySegments":
            case "boundarySegments":
            {
                value = instance.m_boundarySegments;
                return true;
            }
            case "m_cellKey":
            case "cellKey":
            {
                value = instance.m_cellKey;
                return true;
            }
            case "m_edgeIndex":
            case "edgeIndex":
            {
                value = instance.m_edgeIndex;
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
                if (value is not Havok.hk2018.hkaiGatePathUtil.Gate castValue) return false;
                instance.m_gate = castValue;
                return true;
            }
            case "m_boundarySegments":
            case "boundarySegments":
            {
                if (value is Havok.hk2018.hkaiGatePath.BoundarySegmentBits castValue)
                {
                    instance.m_boundarySegments = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_boundarySegments = (Havok.hk2018.hkaiGatePath.BoundarySegmentBits)byteValue;
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
