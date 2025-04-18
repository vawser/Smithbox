// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavVolumeGenerationSettingsChunkSettingsData : HavokData<hkaiNavVolumeGenerationSettings.ChunkSettings> 
{
    public hkaiNavVolumeGenerationSettingsChunkSettingsData(HavokType type, hkaiNavVolumeGenerationSettings.ChunkSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxChunkSizeX":
            case "maxChunkSizeX":
            {
                if (instance.m_maxChunkSizeX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxChunkSizeY":
            case "maxChunkSizeY":
            {
                if (instance.m_maxChunkSizeY is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxChunkSizeZ":
            case "maxChunkSizeZ":
            {
                if (instance.m_maxChunkSizeZ is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_doGreedyMergeAfterCombine":
            case "doGreedyMergeAfterCombine":
            {
                if (instance.m_doGreedyMergeAfterCombine is not TGet castValue) return false;
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
            case "m_maxChunkSizeX":
            case "maxChunkSizeX":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxChunkSizeX = castValue;
                return true;
            }
            case "m_maxChunkSizeY":
            case "maxChunkSizeY":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxChunkSizeY = castValue;
                return true;
            }
            case "m_maxChunkSizeZ":
            case "maxChunkSizeZ":
            {
                if (value is not ushort castValue) return false;
                instance.m_maxChunkSizeZ = castValue;
                return true;
            }
            case "m_doGreedyMergeAfterCombine":
            case "doGreedyMergeAfterCombine":
            {
                if (value is not bool castValue) return false;
                instance.m_doGreedyMergeAfterCombine = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
