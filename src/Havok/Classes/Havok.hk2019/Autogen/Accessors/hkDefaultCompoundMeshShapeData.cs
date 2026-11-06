// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkDefaultCompoundMeshShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkDefaultCompoundMeshShape>
{
    public hkDefaultCompoundMeshShapeData(Havok.hk2019.hkDefaultCompoundMeshShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shapes":
            case "shapes":
            {
                value = instance.m_shapes;
                return true;
            }
            case "m_defaultChildTransforms":
            case "defaultChildTransforms":
            {
                value = instance.m_defaultChildTransforms;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
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
            case "m_shapes":
            case "shapes":
            {
                if (value is not List<Havok.hk2019.hkMeshShape?> castValue) return false;
                instance.m_shapes = castValue;
                return true;
            }
            case "m_defaultChildTransforms":
            case "defaultChildTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_defaultChildTransforms = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<Havok.hk2019.hkDefaultCompoundMeshShape.MeshSection> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
