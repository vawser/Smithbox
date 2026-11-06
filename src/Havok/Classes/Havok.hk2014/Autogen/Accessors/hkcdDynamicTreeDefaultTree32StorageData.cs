// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdDynamicTreeDefaultTree32StorageData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdDynamicTreeDefaultTree32Storage>
{
    public hkcdDynamicTreeDefaultTree32StorageData(Havok.hk2014.hkcdDynamicTreeDefaultTree32Storage instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                value = instance.m_firstFree;
                return true;
            }
            case "m_numLeaves":
            case "numLeaves":
            {
                value = instance.m_numLeaves;
                return true;
            }
            case "m_path":
            case "path":
            {
                value = instance.m_path;
                return true;
            }
            case "m_root":
            case "root":
            {
                value = instance.m_root;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<Havok.hk2014.hkcdDynamicTree.Codec32> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_firstFree":
            case "firstFree":
            {
                if (value is not ushort castValue) return false;
                instance.m_firstFree = castValue;
                return true;
            }
            case "m_numLeaves":
            case "numLeaves":
            {
                if (value is not uint castValue) return false;
                instance.m_numLeaves = castValue;
                return true;
            }
            case "m_path":
            case "path":
            {
                if (value is not uint castValue) return false;
                instance.m_path = castValue;
                return true;
            }
            case "m_root":
            case "root":
            {
                if (value is not ushort castValue) return false;
                instance.m_root = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
