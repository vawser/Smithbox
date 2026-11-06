// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkSkinnedRefMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkSkinnedRefMeshShape>
{
    public hkSkinnedRefMeshShapeData(Havok.hk2019.hkSkinnedRefMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skinnedMeshShape":
            case "skinnedMeshShape":
            {
                value = instance.m_skinnedMeshShape;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                value = instance.m_bones;
                return true;
            }
            case "m_localFromRootTransforms":
            case "localFromRootTransforms":
            {
                value = instance.m_localFromRootTransforms;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
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
            case "m_skinnedMeshShape":
            case "skinnedMeshShape":
            {
                if (value is null)
                {
                    instance.m_skinnedMeshShape = default;
                    return true;
                }
                if (value is Havok.hk2019.hkSkinnedMeshShape castValue)
                {
                    instance.m_skinnedMeshShape = castValue;
                    return true;
                }
                return false;
            }
            case "m_bones":
            case "bones":
            {
                if (value is not List<short> castValue) return false;
                instance.m_bones = castValue;
                return true;
            }
            case "m_localFromRootTransforms":
            case "localFromRootTransforms":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_localFromRootTransforms = castValue;
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
            default:
            return false;
        }
    }

}
