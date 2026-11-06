// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVolumeConstraintData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVolumeConstraint>
{
    public hclVolumeConstraintData(Havok.hk2018.hclVolumeConstraint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_constraintId":
            case "constraintId":
            {
                value = instance.m_constraintId;
                return true;
            }
            case "m_frameDatas":
            case "frameDatas":
            {
                value = instance.m_frameDatas;
                return true;
            }
            case "m_applyDatas":
            case "applyDatas":
            {
                value = instance.m_applyDatas;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_constraintId":
            case "constraintId":
            {
                if (value is not Havok.hk2018.hkHandle<uint> castValue) return false;
                instance.m_constraintId = castValue;
                return true;
            }
            case "m_frameDatas":
            case "frameDatas":
            {
                if (value is not List<Havok.hk2018.hclVolumeConstraint.FrameData> castValue) return false;
                instance.m_frameDatas = castValue;
                return true;
            }
            case "m_applyDatas":
            case "applyDatas":
            {
                if (value is not List<Havok.hk2018.hclVolumeConstraint.ApplyData> castValue) return false;
                instance.m_applyDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
