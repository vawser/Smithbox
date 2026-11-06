// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolume.Edge>
{
    public hkaiNavVolumeEdgeData(Havok.hk2019.hkaiNavVolume.Edge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userEdgeInfoIndex":
            case "userEdgeInfoIndex":
            {
                value = instance.m_userEdgeInfoIndex;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_oppositeCell":
            case "oppositeCell":
            {
                value = instance.m_oppositeCell;
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
                if (value is Havok.hk2019.hkaiNavVolume.CellEdgeFlagBits castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2019.hkaiNavVolume.CellEdgeFlagBits)byteValue;
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
