// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkcdStaticPvsData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkcdStaticPvs>
{
    public hkcdStaticPvsData(Havok.hk2014.hkcdStaticPvs instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_cells":
            case "cells":
            {
                value = instance.m_cells;
                return true;
            }
            case "m_bytesPerCells":
            case "bytesPerCells":
            {
                value = instance.m_bytesPerCells;
                return true;
            }
            case "m_cellsPerBlock":
            case "cellsPerBlock":
            {
                value = instance.m_cellsPerBlock;
                return true;
            }
            case "m_pvs":
            case "pvs":
            {
                value = instance.m_pvs;
                return true;
            }
            case "m_map":
            case "map":
            {
                value = instance.m_map;
                return true;
            }
            case "m_blocks":
            case "blocks":
            {
                value = instance.m_blocks;
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
            case "m_cells":
            case "cells":
            {
                if (value is not Havok.hk2014.hkcdStaticTreeTreehkcdStaticTreeDynamicStorage6 castValue) return false;
                instance.m_cells = castValue;
                return true;
            }
            case "m_bytesPerCells":
            case "bytesPerCells":
            {
                if (value is not int castValue) return false;
                instance.m_bytesPerCells = castValue;
                return true;
            }
            case "m_cellsPerBlock":
            case "cellsPerBlock":
            {
                if (value is not int castValue) return false;
                instance.m_cellsPerBlock = castValue;
                return true;
            }
            case "m_pvs":
            case "pvs":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_pvs = castValue;
                return true;
            }
            case "m_map":
            case "map":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_map = castValue;
                return true;
            }
            case "m_blocks":
            case "blocks":
            {
                if (value is not List<Havok.hk2014.hkcdStaticPvsBlockHeader> castValue) return false;
                instance.m_blocks = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
