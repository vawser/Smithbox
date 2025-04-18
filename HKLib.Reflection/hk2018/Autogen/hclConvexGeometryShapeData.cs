// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclConvexGeometryShapeData : HavokData<hclConvexGeometryShape> 
{
    public hclConvexGeometryShapeData(HavokType type, hclConvexGeometryShape instance) : base(type, instance) {}

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
            case "m_tetrahedraGrid":
            case "tetrahedraGrid":
            {
                if (instance.m_tetrahedraGrid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gridCells":
            case "gridCells":
            {
                if (instance.m_gridCells is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tetrahedraEquations":
            case "tetrahedraEquations":
            {
                if (instance.m_tetrahedraEquations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_localFromWorld":
            case "localFromWorld":
            {
                if (instance.m_localFromWorld is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_worldFromLocal":
            case "worldFromLocal":
            {
                if (instance.m_worldFromLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_objAabb":
            case "objAabb":
            {
                if (instance.m_objAabb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_geomCentroid":
            case "geomCentroid":
            {
                if (instance.m_geomCentroid is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_invCellSize":
            case "invCellSize":
            {
                if (instance.m_invCellSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gridRes":
            case "gridRes":
            {
                if (instance.m_gridRes is not TGet castValue) return false;
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
                if (value is not hkAabb castValue) return false;
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
