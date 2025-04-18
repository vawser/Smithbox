// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiNavMeshGeneration;

namespace HKLib.Reflection.hk2018;

// Seemingly two versions of the same type, one is unused (not referenced in any record type), both were present in the Elden Ring executable

internal class hkaiNavMeshGeneration_SnapshotData : HavokData<Snapshot>
{
    public hkaiNavMeshGeneration_SnapshotData(HavokType type, Snapshot instance) :
        base(type, instance) { }

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
            case "m_geometrySource":
            case "geometrySource":
            {
                if (instance.m_geometrySource is null)
                {
                    return true;
                }

                if (instance.m_geometrySource is TGet castValue)
                {
                    value = castValue;
                    return true;
                }

                return false;
            }
            case "m_settings":
            case "settings":
            {
                if (instance.m_settings is not TGet castValue) return false;
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
            case "m_geometrySource":
            case "geometrySource":
            {
                if (value is null)
                {
                    instance.m_geometrySource = default;
                    return true;
                }

                if (value is GeometrySource castValue)
                {
                    instance.m_geometrySource = castValue;
                    return true;
                }

                return false;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not Settings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            default:
                return false;
        }
    }
}

internal class hkaiNavMeshGenerationSnapshotData : HavokData<hkaiNavMeshGenerationSnapshot>
{
    public hkaiNavMeshGenerationSnapshotData(HavokType type, hkaiNavMeshGenerationSnapshot instance) :
        base(type, instance) { }

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_geometry":
            case "geometry":
            {
                if (instance.m_geometry is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (instance.m_settings is not TGet castValue) return false;
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
            case "m_geometry":
            case "geometry":
            {
                if (value is not hkGeometry castValue) return false;
                instance.m_geometry = castValue;
                return true;
            }
            case "m_settings":
            case "settings":
            {
                if (value is not hkaiNavMeshGenerationUtilsSettings castValue) return false;
                instance.m_settings = castValue;
                return true;
            }
            default:
                return false;
        }
    }
}