#nullable enable
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer;

/// <summary>
/// Reads the .metaparam file that sits next to a game shader in its .shaderbdle.
/// It describes the shader's textures, of which only the UV group is needed here.
/// </summary>
public static class MaterialMetaparam
{
    private const int FirstEntryOffset = 0x98;
    private const int EntrySize = 0x30;
    private const int GroupNameOffset = 0x18;

    private static readonly Regex SlotName = new(@"Texture2D_(\d+)_", RegexOptions.Compiled);

    /// <summary>
    /// Maps each texture slot (the N of Texture2D_N) that has a UV group to the name of that group.
    /// </summary>
    public static Dictionary<int, string> ReadTextureGroups(byte[] data)
    {
        Dictionary<int, string> groups = new();

        if (data.Length < FirstEntryOffset + EntrySize)
        {
            return groups;
        }

        // The strings come straight after the entries, starting with the name of the first
        var entriesEnd = Math.Min(BitConverter.ToInt64(data, FirstEntryOffset), data.Length);

        // Textures are followed by entries for other things, which aren't laid out the same
        for (var entry = FirstEntryOffset; entry + EntrySize <= entriesEnd; entry += EntrySize)
        {
            var nameOffset = BitConverter.ToInt64(data, entry);

            if (nameOffset < entriesEnd || nameOffset >= data.Length)
            {
                break;
            }

            Match slot = SlotName.Match(ReadString(data, nameOffset));

            if (!slot.Success)
            {
                continue;
            }

            var groupOffset = BitConverter.ToInt64(data, entry + GroupNameOffset);

            if (groupOffset < entriesEnd || groupOffset >= data.Length)
            {
                continue;
            }

            var group = ReadString(data, groupOffset);

            if (group.Length > 0)
            {
                groups[int.Parse(slot.Groups[1].Value)] = group;
            }
        }

        return groups;
    }

    private static string ReadString(byte[] data, long offset)
    {
        var start = (int)offset;
        var end = start;

        while (end + 1 < data.Length && (data[end] != 0 || data[end + 1] != 0))
        {
            end += 2;
        }

        return Encoding.Unicode.GetString(data, start, end - start);
    }
}
