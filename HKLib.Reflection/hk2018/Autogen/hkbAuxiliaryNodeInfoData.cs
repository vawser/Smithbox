// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAuxiliaryNodeInfoData : HavokData<hkbAuxiliaryNodeInfo> 
{
    public hkbAuxiliaryNodeInfoData(HavokType type, hkbAuxiliaryNodeInfo instance) : base(type, instance) {}

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
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_depth":
            case "depth":
            {
                if (instance.m_depth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceBehaviorName":
            case "referenceBehaviorName":
            {
                if (instance.m_referenceBehaviorName is null)
                {
                    return true;
                }
                if (instance.m_referenceBehaviorName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_selfTransitionNames":
            case "selfTransitionNames":
            {
                if (instance.m_selfTransitionNames is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkbToolNodeType.NodeType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkbToolNodeType.NodeType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_depth":
            case "depth":
            {
                if (value is not byte castValue) return false;
                instance.m_depth = castValue;
                return true;
            }
            case "m_referenceBehaviorName":
            case "referenceBehaviorName":
            {
                if (value is null)
                {
                    instance.m_referenceBehaviorName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_referenceBehaviorName = castValue;
                    return true;
                }
                return false;
            }
            case "m_selfTransitionNames":
            case "selfTransitionNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_selfTransitionNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
