// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class fsnpCustomMeshParameterData : Havok.Formats.HavokFieldAccess<Havok.hk2016.fsnpCustomMeshParameter>
{
    public fsnpCustomMeshParameterData(Havok.hk2016.fsnpCustomMeshParameter instance) : base(instance) { }

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
            case "m_triangleDataArray":
            case "triangleDataArray":
            {
                value = instance.m_triangleDataArray;
                return true;
            }
            case "m_primitiveDataArray":
            case "primitiveDataArray":
            {
                value = instance.m_primitiveDataArray;
                return true;
            }
            case "m_vertexDataStride":
            case "vertexDataStride":
            {
                value = instance.m_vertexDataStride;
                return true;
            }
            case "m_triangleDataStride":
            case "triangleDataStride":
            {
                value = instance.m_triangleDataStride;
                return true;
            }
            case "m_version":
            case "version":
            {
                value = instance.m_version;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_triangleDataArray":
            case "triangleDataArray":
            {
                if (value is not List<Havok.hk2016.fsnpCustomMeshParameter.TriangleData> castValue) return false;
                instance.m_triangleDataArray = castValue;
                return true;
            }
            case "m_primitiveDataArray":
            case "primitiveDataArray":
            {
                if (value is not List<Havok.hk2016.fsnpCustomMeshParameter.PrimitiveData> castValue) return false;
                instance.m_primitiveDataArray = castValue;
                return true;
            }
            case "m_vertexDataStride":
            case "vertexDataStride":
            {
                if (value is not int castValue) return false;
                instance.m_vertexDataStride = castValue;
                return true;
            }
            case "m_triangleDataStride":
            case "triangleDataStride":
            {
                if (value is not int castValue) return false;
                instance.m_triangleDataStride = castValue;
                return true;
            }
            case "m_version":
            case "version":
            {
                if (value is not uint castValue) return false;
                instance.m_version = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
