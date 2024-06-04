// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkaiNavMeshEdgeZipper;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshEdgeZipperZipEdgesSettingsData : HavokData<ZipEdgesSettings> 
{
    public hkaiNavMeshEdgeZipperZipEdgesSettingsData(HavokType type, ZipEdgesSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edgesSorted":
            case "edgesSorted":
            {
                if (instance.m_edgesSorted is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clamp":
            case "clamp":
            {
                if (instance.m_clamp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tolerance":
            case "tolerance":
            {
                if (instance.m_tolerance is not TGet castValue) return false;
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
            case "m_edgesSorted":
            case "edgesSorted":
            {
                if (value is not bool castValue) return false;
                instance.m_edgesSorted = castValue;
                return true;
            }
            case "m_clamp":
            case "clamp":
            {
                if (value is not bool castValue) return false;
                instance.m_clamp = castValue;
                return true;
            }
            case "m_tolerance":
            case "tolerance":
            {
                if (value is not float castValue) return false;
                instance.m_tolerance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
