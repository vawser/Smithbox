// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclConvexPlanesShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclConvexPlanesShape>
{
    public hclConvexPlanesShapeData(Havok.hk2014.hclConvexPlanesShape instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_planeEquations":
            case "planeEquations":
            {
                value = instance.m_planeEquations;
                return true;
            }
            case "m_localFromWorld":
            case "localFromWorld":
            {
                value = instance.m_localFromWorld;
                return true;
            }
            case "m_worldFromLocal":
            case "worldFromLocal":
            {
                value = instance.m_worldFromLocal;
                return true;
            }
            case "m_objAabb":
            case "objAabb":
            {
                value = instance.m_objAabb;
                return true;
            }
            case "m_geomCentroid":
            case "geomCentroid":
            {
                value = instance.m_geomCentroid;
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
            case "m_planeEquations":
            case "planeEquations":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_planeEquations = castValue;
                return true;
            }
            case "m_localFromWorld":
            case "localFromWorld":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_localFromWorld = castValue;
                return true;
            }
            case "m_worldFromLocal":
            case "worldFromLocal":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_worldFromLocal = castValue;
                return true;
            }
            case "m_objAabb":
            case "objAabb":
            {
                if (value is not Havok.hk2014.hkAabb castValue) return false;
                instance.m_objAabb = castValue;
                return true;
            }
            case "m_geomCentroid":
            case "geomCentroid":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_geomCentroid = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
