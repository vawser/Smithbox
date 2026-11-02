// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiUserEdgeUtilsObbData : HavokData<HKLib.hk2019.hkaiUserEdgeUtils.Obb> 
{
    public hkaiUserEdgeUtilsObbData(HavokType type, HKLib.hk2019.hkaiUserEdgeUtils.Obb instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_halfExtents":
            case "halfExtents":
            {
                if (instance.m_halfExtents is not TGet castValue) return false;
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
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_halfExtents":
            case "halfExtents":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_halfExtents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
