// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingSetVolumeConnectionData : HavokData<hkaiStreamingSet.VolumeConnection> 
{
    public hkaiStreamingSetVolumeConnectionData(HavokType type, hkaiStreamingSet.VolumeConnection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aCellIndex":
            case "aCellIndex":
            {
                if (instance.m_aCellIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bCellIndex":
            case "bCellIndex":
            {
                if (instance.m_bCellIndex is not TGet castValue) return false;
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
            case "m_aCellIndex":
            case "aCellIndex":
            {
                if (value is not int castValue) return false;
                instance.m_aCellIndex = castValue;
                return true;
            }
            case "m_bCellIndex":
            case "bCellIndex":
            {
                if (value is not int castValue) return false;
                instance.m_bCellIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
