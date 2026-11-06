// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclVolumeConstraintMxData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclVolumeConstraintMx>
{
    public hclVolumeConstraintMxData(Havok.hk2018.hclVolumeConstraintMx instance) : base(instance) { }

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
            case "m_frameBatchDatas":
            case "frameBatchDatas":
            {
                value = instance.m_frameBatchDatas;
                return true;
            }
            case "m_frameSingleDatas":
            case "frameSingleDatas":
            {
                value = instance.m_frameSingleDatas;
                return true;
            }
            case "m_applyBatchDatas":
            case "applyBatchDatas":
            {
                value = instance.m_applyBatchDatas;
                return true;
            }
            case "m_applySingleDatas":
            case "applySingleDatas":
            {
                value = instance.m_applySingleDatas;
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
            case "m_frameBatchDatas":
            case "frameBatchDatas":
            {
                if (value is not List<Havok.hk2018.hclVolumeConstraintMx.FrameBatchData> castValue) return false;
                instance.m_frameBatchDatas = castValue;
                return true;
            }
            case "m_frameSingleDatas":
            case "frameSingleDatas":
            {
                if (value is not List<Havok.hk2018.hclVolumeConstraintMx.FrameSingleData> castValue) return false;
                instance.m_frameSingleDatas = castValue;
                return true;
            }
            case "m_applyBatchDatas":
            case "applyBatchDatas":
            {
                if (value is not List<Havok.hk2018.hclVolumeConstraintMx.ApplyBatchData> castValue) return false;
                instance.m_applyBatchDatas = castValue;
                return true;
            }
            case "m_applySingleDatas":
            case "applySingleDatas":
            {
                if (value is not List<Havok.hk2018.hclVolumeConstraintMx.ApplySingleData> castValue) return false;
                instance.m_applySingleDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
