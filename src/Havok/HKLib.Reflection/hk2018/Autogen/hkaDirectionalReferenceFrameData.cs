// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaDirectionalReferenceFrameData : HavokData<hkaDirectionalReferenceFrame> 
{
    public hkaDirectionalReferenceFrameData(HavokType type, hkaDirectionalReferenceFrame instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                if (instance.m_forward is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceFrameSamples":
            case "referenceFrameSamples":
            {
                if (instance.m_referenceFrameSamples is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_movementDir":
            case "movementDir":
            {
                if (instance.m_movementDir is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_forward":
            case "forward":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_forward = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            case "m_referenceFrameSamples":
            case "referenceFrameSamples":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_referenceFrameSamples = castValue;
                return true;
            }
            case "m_movementDir":
            case "movementDir":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_movementDir = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
