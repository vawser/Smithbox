// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkDefaultCompoundMeshBodyData : HavokData<hkDefaultCompoundMeshBody> 
{
    public hkDefaultCompoundMeshBodyData(HavokType type, hkDefaultCompoundMeshBody instance) : base(type, instance) {}

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
            case "m_bodies":
            case "bodies":
            {
                if (instance.m_bodies is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (instance.m_transform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (instance.m_shape is null)
                {
                    return true;
                }
                if (instance.m_shape is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformSet":
            case "transformSet":
            {
                if (instance.m_transformSet is null)
                {
                    return true;
                }
                if (instance.m_transformSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformIsDirty":
            case "transformIsDirty":
            {
                if (instance.m_transformIsDirty is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformSetUpdated":
            case "transformSetUpdated":
            {
                if (instance.m_transformSetUpdated is not TGet castValue) return false;
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
            case "m_bodies":
            case "bodies":
            {
                if (value is not List<hkMeshBody?> castValue) return false;
                instance.m_bodies = castValue;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transform = castValue;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                if (value is null)
                {
                    instance.m_shape = default;
                    return true;
                }
                if (value is hkDefaultCompoundMeshShape castValue)
                {
                    instance.m_shape = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformSet":
            case "transformSet":
            {
                if (value is null)
                {
                    instance.m_transformSet = default;
                    return true;
                }
                if (value is hkIndexedTransformSet castValue)
                {
                    instance.m_transformSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_transformIsDirty":
            case "transformIsDirty":
            {
                if (value is not bool castValue) return false;
                instance.m_transformIsDirty = castValue;
                return true;
            }
            case "m_transformSetUpdated":
            case "transformSetUpdated":
            {
                if (value is not bool castValue) return false;
                instance.m_transformSetUpdated = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
