// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiNavVolumeGenerationSettingsChunkSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiNavVolumeGenerationSettings.ChunkSettings>
{
    public hkaiNavVolumeGenerationSettingsChunkSettingsData(Havok.hk2018.hkaiNavVolumeGenerationSettings.ChunkSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxChunkSizeX":
            case "maxChunkSizeX":
            {
                value = instance.m_maxChunkSizeX;
                return true;
            }
            case "m_maxChunkSizeY":
            case "maxChunkSizeY":
            {
                value = instance.m_maxChunkSizeY;
                return true;
            }
            case "m_maxChunkSizeZ":
            case "maxChunkSizeZ":
            {
                value = instance.m_maxChunkSizeZ;
                return true;
            }
            case "m_doGreedyMergeAfterCombine":
            case "doGreedyMergeAfterCombine":
            {
                value = instance.m_doGreedyMergeAfterCombine;
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
