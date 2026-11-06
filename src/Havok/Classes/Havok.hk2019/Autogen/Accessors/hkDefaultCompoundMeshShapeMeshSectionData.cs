// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkDefaultCompoundMeshShapeMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkDefaultCompoundMeshShape.MeshSection>
{
    public hkDefaultCompoundMeshShapeMeshSectionData(Havok.hk2019.hkDefaultCompoundMeshShape.MeshSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_shapeIndex":
            case "shapeIndex":
            {
                value = instance.m_shapeIndex;
                return true;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                value = instance.m_sectionIndex;
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
            case "m_shapeIndex":
            case "shapeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_shapeIndex = castValue;
                return true;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
