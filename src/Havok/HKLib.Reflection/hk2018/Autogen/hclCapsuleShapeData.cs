// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclCapsuleShapeData : HavokData<hclCapsuleShape> 
{
    public hclCapsuleShapeData(HavokType type, hclCapsuleShape instance) : base(type, instance) {}

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
            case "m_start":
            case "start":
            {
                if (instance.m_start is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (instance.m_end is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dir":
            case "dir":
            {
                if (instance.m_dir is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (instance.m_radius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capLenSqrdInv":
            case "capLenSqrdInv":
            {
                if (instance.m_capLenSqrdInv is not TGet castValue) return false;
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
            case "m_start":
            case "start":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_start = castValue;
                return true;
            }
            case "m_end":
            case "end":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_end = castValue;
                return true;
            }
            case "m_dir":
            case "dir":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_dir = castValue;
                return true;
            }
            case "m_radius":
            case "radius":
            {
                if (value is not float castValue) return false;
                instance.m_radius = castValue;
                return true;
            }
            case "m_capLenSqrdInv":
            case "capLenSqrdInv":
            {
                if (value is not float castValue) return false;
                instance.m_capLenSqrdInv = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
