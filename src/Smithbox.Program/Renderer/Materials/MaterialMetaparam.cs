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
    private const int TextureCountOffset = 0x0C;
    private const int FirstTextureOffset = 0x98;
    private const int TextureSize = 0x30;
    private const int GroupNameOffset = 0x18;

    private static readonly Regex SlotName = new(@"Texture2D_(\d+)_", RegexOptions.Compiled);

    /// <summary>
    /// Maps each texture slot (the N of Texture2D_N) that has a UV group to the name of that group.
    /// </summary>
    public static Dictionary<int, string> ReadTextureGroups(byte[] data)
    {
        Dictionary<int, string> groups = new();

        var count = BitConverter.ToInt32(data, TextureCountOffset);

        for (var i = 0; i < count; i++)
        {
            var entry = FirstTextureOffset + (i * TextureSize);

            Match slot = SlotName.Match(ReadString(data, BitConverter.ToInt64(data, entry)));
            var group = ReadString(data, BitConverter.ToInt64(data, entry + GroupNameOffset));

            if (slot.Success && group.Length > 0)
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
