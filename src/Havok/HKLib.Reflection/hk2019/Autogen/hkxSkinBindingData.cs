// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxSkinBindingData : HavokData<HKLib.hk2019.hkxSkinBinding> 
{
    public hkxSkinBindingData(HavokType type, HKLib.hk2019.hkxSkinBinding instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mesh":
            case "mesh":
            {
                if (instance.m_mesh is null)
                {
                    return true;
                }
                if (instance.m_mesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeNames":
            case "nodeNames":
            {
                if (instance.m_nodeNames is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                if (instance.m_bindPose is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initSkinTransform":
            case "initSkinTransform":
            {
                if (instance.m_initSkinTransform is not TGet castValue) return false;
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
            case "m_mesh":
            case "mesh":
            {
                if (value is null)
                {
                    instance.m_mesh = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkxMesh castValue)
                {
                    instance.m_mesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_nodeNames":
            case "nodeNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_nodeNames = castValue;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_bindPose = castValue;
                return true;
            }
            case "m_initSkinTransform":
            case "initSkinTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_initSkinTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
