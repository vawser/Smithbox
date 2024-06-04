// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeEdgeData : HavokData<hkaiNavVolume.Edge> 
{
    public hkaiNavVolumeEdgeData(HavokType type, hkaiNavVolume.Edge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userEdgeInfoIndex":
            case "userEdgeInfoIndex":
            {
                if (instance.m_userEdgeInfoIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_oppositeCell":
            case "oppositeCell":
            {
                if (instance.m_oppositeCell is not TGet castValue) return false;
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
            case "m_userEdgeInfoIndex":
            case "userEdgeInfoIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_userEdgeInfoIndex = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hkaiNavVolume.CellEdgeFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (hkaiNavVolume.CellEdgeFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_oppositeCell":
            case "oppositeCell":
            {
                if (value is not uint castValue) return false;
                instance.m_oppositeCell = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
