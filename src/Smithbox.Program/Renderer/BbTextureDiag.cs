#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;

namespace StudioCore.Renderer;

/// <summary>
/// TEMPORARY: traces the textures of the BB map piece m21_00_00_00_001010 from the FLVER's lookup, through the PS4
/// upload, to the material receiving them, to find why some of its meshes draw untextured.
/// Writes bb_texture_diag.log beside the executable. Remove once the cause is known.
/// </summary>
internal static class BbTextureDiag
{
    private static readonly object Lock = new();
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "bb_texture_diag.log");
    private static bool _started;

    /// <summary>Texture names the traced piece asked for, so their uploads can be traced too</summary>
    public static readonly ConcurrentDictionary<string, byte> TracedTextures = new(StringComparer.OrdinalIgnoreCase);

    public static bool IsTracedModel(string? name) =>
        name != null && name.Contains("001010", StringComparison.OrdinalIgnoreCase);

    public static bool IsTracedTexture(string? name) => name != null && TracedTextures.ContainsKey(name);

    /// <summary>Descriptor slots of the traced textures, so the descriptor writes for them can be traced</summary>
    public static readonly ConcurrentDictionary<uint, string> TracedSlots = new();

    public static void Write(string message)
    {
        lock (Lock)
        {
            try
            {
                if (!_started)
                {
                    File.WriteAllText(LogPath, $"BB texture diag started {DateTime.Now:O}{Environment.NewLine}");
                    _started = true;
                }

                File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} [t{Environment.CurrentManagedThreadId}] {message}{Environment.NewLine}");
            }
            catch
            {
                // Diagnostics must never break rendering
            }
        }
    }
}
