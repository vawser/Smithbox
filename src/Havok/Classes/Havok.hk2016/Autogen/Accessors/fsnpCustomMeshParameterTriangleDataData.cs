// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class fsnpCustomMeshParameterTriangleDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.fsnpCustomMeshParameter.TriangleData>
{
    public fsnpCustomMeshParameterTriangleDataData(Havok.hk2016.fsnpCustomMeshParameter.TriangleData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_primitiveDataIndex":
            case "primitiveDataIndex":
            {
                value = instance.m_primitiveDataIndex;
                return true;
            }
            case "m_triangleDataIndex":
            case "triangleDataIndex":
            {
                value = instance.m_triangleDataIndex;
                return true;
            }
            case "m_vertexIndexA":
            case "vertexIndexA":
            {
                value = instance.m_vertexIndexA;
                return true;
            }
            case "m_vertexIndexB":
            case "vertexIndexB":
            {
                value = instance.m_vertexIndexB;
                return true;
            }
            case "m_vertexIndexC":
            case "vertexIndexC":
            {
                value = instance.m_vertexIndexC;
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
            case "m_primitiveDataIndex":
            case "primitiveDataIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_primitiveDataIndex = castValue;
                return true;
            }
            case "m_triangleDataIndex":
            case "triangleDataIndex":
            {
                if (value is not uint castValue) return false;
                instance.m_triangleDataIndex = castValue;
                return true;
            }
            case "m_vertexIndexA":
            case "vertexIndexA":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexIndexA = castValue;
                return true;
            }
            case "m_vertexIndexB":
            case "vertexIndexB":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexIndexB = castValue;
                return true;
            }
            case "m_vertexIndexC":
            case "vertexIndexC":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexIndexC = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
