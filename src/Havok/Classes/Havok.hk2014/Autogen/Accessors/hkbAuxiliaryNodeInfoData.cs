// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbAuxiliaryNodeInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbAuxiliaryNodeInfo>
{
    public hkbAuxiliaryNodeInfoData(Havok.hk2014.hkbAuxiliaryNodeInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_depth":
            case "depth":
            {
                value = instance.m_depth;
                return true;
            }
            case "m_referenceBehaviorName":
            case "referenceBehaviorName":
            {
                value = instance.m_referenceBehaviorName;
                return true;
            }
            case "m_selfTransitionNames":
            case "selfTransitionNames":
            {
                value = instance.m_selfTransitionNames;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkbToolNodeType.NodeType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2014.hkbToolNodeType.NodeType)ushortValue;
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
