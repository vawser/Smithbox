// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018.hkcdStaticMeshTree;

namespace HKLib.Reflection.hk2018;

internal class hkcdStaticMeshTreeConnectivitySectionHeaderData : HavokData<Connectivity.SectionHeader> 
{
    public hkcdStaticMeshTreeConnectivitySectionHeaderData(HavokType type, Connectivity.SectionHeader instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_baseLocal":
            case "baseLocal":
            {
                if (instance.m_baseLocal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_baseGlobal":
            case "baseGlobal":
            {
                if (instance.m_baseGlobal is not TGet castValue) return false;
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
            case "m_baseLocal":
            case "baseLocal":
            {
                if (value is not uint castValue) return false;
                instance.m_baseLocal = castValue;
                return true;
            }
            case "m_baseGlobal":
            case "baseGlobal":
            {
                if (value is not uint castValue) return false;
                instance.m_baseGlobal = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
