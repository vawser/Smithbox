// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdStaticMeshTreeConnectivitySectionHeaderData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdStaticMeshTree.Connectivity.SectionHeader>
{
    public hkcdStaticMeshTreeConnectivitySectionHeaderData(Havok.hk2018.hkcdStaticMeshTree.Connectivity.SectionHeader instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_baseLocal":
            case "baseLocal":
            {
                value = instance.m_baseLocal;
                return true;
            }
            case "m_baseGlobal":
            case "baseGlobal":
            {
                value = instance.m_baseGlobal;
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
