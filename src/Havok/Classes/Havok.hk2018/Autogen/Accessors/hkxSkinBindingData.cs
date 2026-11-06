// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkxSkinBindingData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkxSkinBinding>
{
    public hkxSkinBindingData(Havok.hk2018.hkxSkinBinding instance) : base(instance) { }

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
            case "m_mesh":
            case "mesh":
            {
                value = instance.m_mesh;
                return true;
            }
            case "m_nodeNames":
            case "nodeNames":
            {
                value = instance.m_nodeNames;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                value = instance.m_bindPose;
                return true;
            }
            case "m_initSkinTransform":
            case "initSkinTransform":
            {
                value = instance.m_initSkinTransform;
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
            case "m_mesh":
            case "mesh":
            {
                if (value is null)
                {
                    instance.m_mesh = default;
                    return true;
                }
                if (value is Havok.hk2018.hkxMesh castValue)
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
