// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclTransitionConstraintSetData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclTransitionConstraintSet>
{
    public hclTransitionConstraintSetData(Havok.hk2018.hclTransitionConstraintSet instance) : base(instance) { }

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
            case "m_perParticleData":
            case "perParticleData":
            {
                value = instance.m_perParticleData;
                return true;
            }
            case "m_toAnimPeriod":
            case "toAnimPeriod":
            {
                value = instance.m_toAnimPeriod;
                return true;
            }
            case "m_toAnimPlusDelayPeriod":
            case "toAnimPlusDelayPeriod":
            {
                value = instance.m_toAnimPlusDelayPeriod;
                return true;
            }
            case "m_toSimPeriod":
            case "toSimPeriod":
            {
                value = instance.m_toSimPeriod;
                return true;
            }
            case "m_toSimPlusDelayPeriod":
            case "toSimPlusDelayPeriod":
            {
                value = instance.m_toSimPlusDelayPeriod;
                return true;
            }
            case "m_referenceMeshBufferIdx":
            case "referenceMeshBufferIdx":
            {
                value = instance.m_referenceMeshBufferIdx;
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
            case "m_perParticleData":
            case "perParticleData":
            {
                if (value is not List<Havok.hk2018.hclTransitionConstraintSet.PerParticle> castValue) return false;
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
