// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkDefaultCompoundMeshBodyData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkDefaultCompoundMeshBody>
{
    public hkDefaultCompoundMeshBodyData(Havok.hk2018.hkDefaultCompoundMeshBody instance) : base(instance) { }

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
            case "m_bodies":
            case "bodies":
            {
                value = instance.m_bodies;
                return true;
            }
            case "m_transform":
            case "transform":
            {
                value = instance.m_transform;
                return true;
            }
            case "m_shape":
            case "shape":
            {
                value = instance.m_shape;
                return true;
            }
            case "m_transformSet":
            case "transformSet":
            {
                value = instance.m_transformSet;
                return true;
            }
            case "m_transformIsDirty":
            case "transformIsDirty":
            {
                value = instance.m_transformIsDirty;
                return true;
            }
            case "m_transformSetUpdated":
            case "transformSetUpdated":
            {
                value = instance.m_transformSetUpdated;
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
            case "m_bodies":
            case "bodies":
            {
                if (value is not List<Havok.hk2018.hkMeshBody?> castValue) return false;
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
                if (value is Havok.hk2018.hkDefaultCompoundMeshShape castValue)
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
                if (value is Havok.hk2018.hkIndexedTransformSet castValue)
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
