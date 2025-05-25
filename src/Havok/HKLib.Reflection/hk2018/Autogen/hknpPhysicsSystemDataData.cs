// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpPhysicsSystemDataData : HavokData<HKLib.hk2018.hknpPhysicsSystemData> 
{
    public hknpPhysicsSystemDataData(HavokType type, HKLib.hk2018.hknpPhysicsSystemData instance) : base(type, instance) {}

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
            case "m_materials":
            case "materials":
            {
                if (instance.m_materials is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motionProperties":
            case "motionProperties":
            {
                if (instance.m_motionProperties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyCinfos":
            case "bodyCinfos":
            {
                if (instance.m_bodyCinfos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintCinfos":
            case "constraintCinfos":
            {
                if (instance.m_constraintCinfos is not TGet castValue) return false;
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
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                if (instance.m_microStepMultiplier is not TGet castValue) return false;
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
            case "m_materials":
            case "materials":
            {
                if (value is not List<hknpMaterial> castValue) return false;
                instance.m_materials = castValue;
                return true;
            }
            case "m_motionProperties":
            case "motionProperties":
            {
                if (value is not List<hknpMotionProperties> castValue) return false;
                instance.m_motionProperties = castValue;
                return true;
            }
            case "m_bodyCinfos":
            case "bodyCinfos":
            {
                if (value is not List<HKLib.hk2018.hknpPhysicsSystemData.bodyCinfoWithAttachment> castValue) return false;
                instance.m_bodyCinfos = castValue;
                return true;
            }
            case "m_constraintCinfos":
            case "constraintCinfos":
            {
                if (value is not List<hknpConstraintCinfo> castValue) return false;
                instance.m_constraintCinfos = castValue;
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
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                if (value is not byte castValue) return false;
                instance.m_microStepMultiplier = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
