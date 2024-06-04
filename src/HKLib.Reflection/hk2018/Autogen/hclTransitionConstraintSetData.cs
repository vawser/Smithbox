// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTransitionConstraintSetData : HavokData<hclTransitionConstraintSet> 
{
    public hclTransitionConstraintSetData(HavokType type, hclTransitionConstraintSet instance) : base(type, instance) {}

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
            case "m_perParticleData":
            case "perParticleData":
            {
                if (instance.m_perParticleData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (instance.m_toAnimPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toAnimPlusDelayPeriod":
            case "toAnimPlusDelayPeriod":
            {
                if (instance.m_toAnimPlusDelayPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (instance.m_toSimPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimPlusDelayPeriod":
            case "toSimPlusDelayPeriod":
            {
                if (instance.m_toSimPlusDelayPeriod is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceMeshBufferIdx":
            case "referenceMeshBufferIdx":
            {
                if (instance.m_referenceMeshBufferIdx is not TGet castValue) return false;
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
            case "m_perParticleData":
            case "perParticleData":
            {
                if (value is not List<hclTransitionConstraintSet.PerParticle> castValue) return false;
                instance.m_perParticleData = castValue;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toAnimPeriod = castValue;
                return true;
            }
            case "m_toAnimPlusDelayPeriod":
            case "toAnimPlusDelayPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toAnimPlusDelayPeriod = castValue;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toSimPeriod = castValue;
                return true;
            }
            case "m_toSimPlusDelayPeriod":
            case "toSimPlusDelayPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_toSimPlusDelayPeriod = castValue;
                return true;
            }
            case "m_referenceMeshBufferIdx":
            case "referenceMeshBufferIdx":
            {
                if (value is not uint castValue) return false;
                instance.m_referenceMeshBufferIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
