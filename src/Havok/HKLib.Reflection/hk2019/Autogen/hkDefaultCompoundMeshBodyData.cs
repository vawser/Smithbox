// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkDefaultCompoundMeshBodyData : HavokData<HKLib.hk2019.hkDefaultCompoundMeshBody> 
{
    public hkDefaultCompoundMeshBodyData(HavokType type, HKLib.hk2019.hkDefaultCompoundMeshBody instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_bodies":
            case "bodies":
            {
                if (value is not List<HKLib.hk2019.hkMeshBody?> castValue) return false;
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
                if (value is HKLib.hk2019.hkDefaultCompoundMeshShape castValue)
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
                if (value is HKLib.hk2019.hkIndexedTransformSet castValue)
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
