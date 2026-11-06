// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclConvexGeometryShapeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclConvexGeometryShape>
{
    public hclConvexGeometryShapeData(Havok.hk2018.hclConvexGeometryShape instance) : base(instance) { }

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
            case "m_tetrahedraGrid":
            case "tetrahedraGrid":
            {
                value = instance.m_tetrahedraGrid;
                return true;
            }
            case "m_gridCells":
            case "gridCells":
            {
                value = instance.m_gridCells;
                return true;
            }
            case "m_tetrahedraEquations":
            case "tetrahedraEquations":
            {
                value = instance.m_tetrahedraEquations;
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
            case "m_invCellSize":
            case "invCellSize":
            {
                value = instance.m_invCellSize;
                return true;
            }
            case "m_gridRes":
            case "gridRes":
            {
                value = instance.m_gridRes;
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
            case "m_tetrahedraGrid":
            case "tetrahedraGrid":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_tetrahedraGrid = castValue;
                return true;
            }
            case "m_gridCells":
            case "gridCells":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_gridCells = castValue;
                return true;
            }
            case "m_tetrahedraEquations":
            case "tetrahedraEquations":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_tetrahedraEquations = castValue;
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
                if (value is not Havok.hk2018.hkAabb castValue) return false;
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
            case "m_invCellSize":
            case "invCellSize":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_invCellSize = castValue;
                return true;
            }
            case "m_gridRes":
            case "gridRes":
            {
                if (value is not ushort castValue) return false;
                instance.m_gridRes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
