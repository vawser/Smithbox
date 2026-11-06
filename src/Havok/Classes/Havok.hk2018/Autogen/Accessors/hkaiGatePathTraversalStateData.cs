// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiGatePathTraversalStateData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiGatePath.TraversalState>
{
    public hkaiGatePathTraversalStateData(Havok.hk2018.hkaiGatePath.TraversalState instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_curPos":
            case "curPos":
            {
                value = instance.m_curPos;
                return true;
            }
            case "m_curCellIdx":
            case "curCellIdx":
            {
                value = instance.m_curCellIdx;
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
            case "m_curPos":
            case "curPos":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_curPos = castValue;
                return true;
            }
            case "m_curCellIdx":
            case "curCellIdx":
            {
                if (value is not int castValue) return false;
                instance.m_curCellIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
