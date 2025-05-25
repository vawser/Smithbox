// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVolumeConstraintMxData : HavokData<hclVolumeConstraintMx> 
{
    public hclVolumeConstraintMxData(HavokType type, hclVolumeConstraintMx instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_constraintId":
            case "constraintId":
            {
                if (instance.m_constraintId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameBatchDatas":
            case "frameBatchDatas":
            {
                if (instance.m_frameBatchDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameSingleDatas":
            case "frameSingleDatas":
            {
                if (instance.m_frameSingleDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_applyBatchDatas":
            case "applyBatchDatas":
            {
                if (instance.m_applyBatchDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_applySingleDatas":
            case "applySingleDatas":
            {
                if (instance.m_applySingleDatas is not TGet castValue) return false;
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
                if (value is not hkHandle<uint> castValue) return false;
                instance.m_constraintId = castValue;
                return true;
            }
            case "m_frameBatchDatas":
            case "frameBatchDatas":
            {
                if (value is not List<hclVolumeConstraintMx.FrameBatchData> castValue) return false;
                instance.m_frameBatchDatas = castValue;
                return true;
            }
            case "m_frameSingleDatas":
            case "frameSingleDatas":
            {
                if (value is not List<hclVolumeConstraintMx.FrameSingleData> castValue) return false;
                instance.m_frameSingleDatas = castValue;
                return true;
            }
            case "m_applyBatchDatas":
            case "applyBatchDatas":
            {
                if (value is not List<hclVolumeConstraintMx.ApplyBatchData> castValue) return false;
                instance.m_applyBatchDatas = castValue;
                return true;
            }
            case "m_applySingleDatas":
            case "applySingleDatas":
            {
                if (value is not List<hclVolumeConstraintMx.ApplySingleData> castValue) return false;
                instance.m_applySingleDatas = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
