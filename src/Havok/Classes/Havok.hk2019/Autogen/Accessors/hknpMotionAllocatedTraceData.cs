// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMotionAllocatedTraceData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMotionAllocatedTrace>
{
    public hknpMotionAllocatedTraceData(Havok.hk2019.hknpMotionAllocatedTrace instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                value = instance.m_sizePaddedTo16;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                value = instance.m_filterBits;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                value = instance.m_primaryType;
                return true;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                value = instance.m_secondaryType;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                value = instance.m_motionId;
                return true;
            }
            case "m_motionInfo":
            case "motionInfo":
            {
                value = instance.m_motionInfo;
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
            case "m_sizePaddedTo16":
            case "sizePaddedTo16":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizePaddedTo16 = castValue;
                return true;
            }
            case "m_filterBits":
            case "filterBits":
            {
                if (value is not byte castValue) return false;
                instance.m_filterBits = castValue;
                return true;
            }
            case "m_primaryType":
            case "primaryType":
            {
                if (value is Havok.hk2019.hkCommand.PrimaryType castValue)
                {
                    instance.m_primaryType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primaryType = (Havok.hk2019.hkCommand.PrimaryType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_secondaryType":
            case "secondaryType":
            {
                if (value is not ushort castValue) return false;
                instance.m_secondaryType = castValue;
                return true;
            }
            case "m_motionId":
            case "motionId":
            {
                if (value is not Havok.hk2019.hknpMotionId castValue) return false;
                instance.m_motionId = castValue;
                return true;
            }
            case "m_motionInfo":
            case "motionInfo":
            {
                if (value is not Havok.hk2019.hknpMotionCinfo castValue) return false;
                instance.m_motionInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
