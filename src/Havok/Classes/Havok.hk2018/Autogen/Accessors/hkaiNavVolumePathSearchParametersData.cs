// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumePathSearchParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolumePathSearchParameters>
{
    public hkaiNavVolumePathSearchParametersData(Havok.hk2018.hkaiNavVolumePathSearchParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_lineOfSightFlags":
            case "lineOfSightFlags":
            {
                value = instance.m_lineOfSightFlags;
                return true;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                value = instance.m_heuristicWeight;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                value = instance.m_maximumPathLength;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                value = instance.m_bufferSizes;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_lineOfSightFlags":
            case "lineOfSightFlags":
            {
                if (value is Havok.hk2018.hkaiNavVolumePathSearchParameters.LineOfSightFlags castValue)
                {
                    instance.m_lineOfSightFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_lineOfSightFlags = (Havok.hk2018.hkaiNavVolumePathSearchParameters.LineOfSightFlags)byteValue;
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
                if (value is not Havok.hk2018.hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_bufferSizes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
