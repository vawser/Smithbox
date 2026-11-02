// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiNavVolumePathSearchParametersData : HavokData<HKLib.hk2019.hkaiNavVolumePathSearchParameters> 
{
    public hkaiNavVolumePathSearchParametersData(HavokType type, HKLib.hk2019.hkaiNavVolumePathSearchParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lineOfSightType":
            case "lineOfSightType":
            {
                if (instance.m_lineOfSightType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_lineOfSightType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (instance.m_heuristicWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                if (instance.m_maximumPathLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                if (instance.m_bufferSizes is not TGet castValue) return false;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_lineOfSightType":
            case "lineOfSightType":
            {
                if (value is HKLib.hk2019.hkaiNavVolumePathSearchParameters.LineOfSightType castValue)
                {
                    instance.m_lineOfSightType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_lineOfSightType = (HKLib.hk2019.hkaiNavVolumePathSearchParameters.LineOfSightType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (value is not float castValue) return false;
                instance.m_heuristicWeight = castValue;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                if (value is not float castValue) return false;
                instance.m_maximumPathLength = castValue;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                if (value is not HKLib.hk2019.hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_bufferSizes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
