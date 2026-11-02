// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiGatePathTraversalStateData : HavokData<HKLib.hk2019.hkaiGatePath.TraversalState> 
{
    public hkaiGatePathTraversalStateData(HavokType type, HKLib.hk2019.hkaiGatePath.TraversalState instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_curPos":
            case "curPos":
            {
                if (instance.m_curPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_curCellIdx":
            case "curCellIdx":
            {
                if (instance.m_curCellIdx is not TGet castValue) return false;
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
