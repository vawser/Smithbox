using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Logger;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.FileBrowser;

/// <summary>
/// The Item Viewer's preview of the current selection: a thumbnail for a selected texture, the text of a text file,
/// or a hex dump of anything else. Read on a background task; a texture's GPU upload happens on the main thread, as
/// the Texture Viewer and the Material Editor's previews do. Only the current preview is kept.
/// </summary>
public class FilePreview
{
    private readonly FileEditorView Parent;
    private readonly ProjectEntry Project;

    // Files above this are not read just to preview them (a .bdt half or a movie can be gigabytes)
    private const long MaxPreviewFileSize = 32L * 1024 * 1024;
    private const int MaxTextBytes = 64 * 1024;
    private const int MaxHexBytes = 4 * 1024;

    private static readonly string[] TextExtensions =
        [".txt", ".xml", ".lua", ".hks", ".json", ".csv", ".ini", ".cfg", ".log", ".md", ".emeld", ".fmgjson"];

    private enum PreviewKind
    {
        None,
        Loading,
        Texture,
        Text,
        Hex,
        Message
    }

    private sealed class Result
    {
        public int Request;
        public PreviewKind Kind;
        public string Text = "";
        public TPF Tpf;
        public int TextureIndex = -1;
    }

    private string CurrentKey = null;
    private int Request = 0;

    private PreviewKind Kind = PreviewKind.None;
    private string Text = "";
    private TextureResource Texture;
    private string TextureInfo = "";

    private readonly ConcurrentQueue<Result> Finished = new();

    public FilePreview(FileEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display()
    {
        var selection = Parent.Selection;
        var file = selection.SelectedVfsFile;

        if (file == null || selection.IsLoading)
            return;

        var key = $"{file.Path}#{selection.SelectedInternalFile}#{selection.SelectedInternalTexFile}";

        if (key != CurrentKey)
        {
            CurrentKey = key;
            Start(file, selection.SelectedInternalFile, selection.SelectedInternalTexFile, selection.Details);
        }

        Apply();

        GUI.SimpleHeader(
            LOC.Get("FILE_ItemViewer_Preview_Header"),
            LOC.Get("FILE_ItemViewer_Preview_Header_TT"));

        switch (Kind)
        {
            case PreviewKind.Loading:
                ImGui.TextDisabled(LOC.Get("FILE_ItemViewer_Loading"));
                break;

            case PreviewKind.Message:
                ImGui.TextDisabled(Text);
                break;

            case PreviewKind.Texture:
                DisplayTexture();
                break;

            case PreviewKind.Text:
            case PreviewKind.Hex:
                var size = new Vector2(-1, Math.Max(ImGui.GetContentRegionAvail().Y, 120 * DPI.UIScale()));
                ImGui.InputTextMultiline("##filePreviewText", ref Text, GUI.GetTextInputBuffer(Text), size,
                    ImGuiInputTextFlags.ReadOnly);
                break;
        }
    }

    private void DisplayTexture()
    {
        var gpu = Texture?.GPUTexture;

        // The upload completes on a later frame; until then the texture has no size
        if (gpu == null || !gpu.Resident || gpu.Width == 0)
        {
            ImGui.TextDisabled(LOC.Get("FILE_ItemViewer_Loading"));
            return;
        }

        var width = Math.Max(1, (int)gpu.Width);
        var height = Math.Max(1, (int)gpu.Height);
        var fit = Math.Min(ImGui.GetContentRegionAvail().X, 512 * DPI.UIScale());
        var scale = Math.Max(fit, 16) / Math.Max(width, height);

        ImGui.Image(new ImTextureID(gpu.TexHandle), new Vector2(width * scale, height * scale));
        ImGui.Text(TextureInfo);
    }

    /// <summary>
    /// Takes the newest finished read, if it's for the current selection, and uploads a texture. Main thread.
    /// </summary>
    private void Apply()
    {
        while (Finished.TryDequeue(out var result))
        {
            if (result.Request != Request)
                continue;

            Texture?.Dispose();
            Texture = null;

            Kind = result.Kind;
            Text = result.Text;

            if (result.Kind != PreviewKind.Texture)
                continue;

            try
            {
                var resource = new TextureResource(result.Tpf, result.TextureIndex);

                if (resource._LoadTexture(AccessLevel.AccessFull))
                {
                    Texture = resource;

                    var tex = result.Tpf.Textures[result.TextureIndex];
                    TextureInfo = LOC.Get("FILE_ItemViewer_Preview_Texture_Info", tex.Name, tex.Format, result.Tpf.Platform);
                }
                else
                {
                    resource.Dispose();
                    Kind = PreviewKind.Message;
                    Text = LOC.Get("FILE_ItemViewer_Preview_Texture_Failed");
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("FILE_ItemViewer_Preview_Texture_Failed"), e);
                Kind = PreviewKind.Message;
                Text = LOC.Get("FILE_ItemViewer_Preview_Texture_Failed");
            }
        }
    }

    private void Start(FileDictionaryEntry file, string internalKey, string textureName, FileDetails details)
    {
        var request = ++Request;
        Kind = PreviewKind.Loading;

        var fs = Project.VFS.FS;
        var path = file.Path;

        Task.Run(() =>
        {
            var result = new Result { Request = request };

            try
            {
                if (!string.IsNullOrEmpty(textureName))
                {
                    LoadTexture(fs, path, internalKey, textureName, result);
                }
                else
                {
                    Memory<byte>? data;
                    string name;

                    if (!string.IsNullOrEmpty(internalKey))
                    {
                        var container = FileContainerReader.Open(fs, path);
                        data = container != null ? FileContainerReader.ReadByKey(container, internalKey) : null;
                        name = internalKey;
                    }
                    else if (details?.Size > MaxPreviewFileSize)
                    {
                        result.Kind = PreviewKind.Message;
                        result.Text = LOC.Get("FILE_ItemViewer_Preview_Too_Large");
                        Finished.Enqueue(result);
                        return;
                    }
                    else
                    {
                        data = fs.ReadFile(path);
                        name = path;
                    }

                    if (data == null)
                    {
                        result.Kind = PreviewKind.Message;
                        result.Text = LOC.Get("FILE_ItemViewer_Preview_Unavailable");
                    }
                    else
                    {
                        LoadData(data.Value, name, result);
                    }
                }
            }
            catch (Exception e)
            {
                result.Kind = PreviewKind.Message;
                result.Text = LOC.Get("FILE_ItemViewer_Preview_Failed", e.Message);
            }

            Finished.Enqueue(result);
        });
    }

    private static void LoadTexture(Andre.IO.VFS.VirtualFileSystem fs, string path, string internalKey, string textureName, Result result)
    {
        var container = FileContainerReader.Open(fs, path);

        TPF tpf = null;

        if (container?.Kind is FileContainerKind.Tpf)
        {
            tpf = container.Tpf;
        }
        else if (container != null && !string.IsNullOrEmpty(internalKey))
        {
            var data = FileContainerReader.ReadByKey(container, internalKey);
            if (data != null)
                tpf = FileContainerReader.ReadTpf(data.Value);
        }

        var index = tpf?.Textures.FindIndex(t => t.Name == textureName) ?? -1;

        if (index < 0)
        {
            result.Kind = PreviewKind.Message;
            result.Text = LOC.Get("FILE_ItemViewer_Preview_Unavailable");
            return;
        }

        result.Kind = PreviewKind.Texture;
        result.Tpf = tpf;
        result.TextureIndex = index;
    }

    /// <summary>
    /// Text for text files (by extension, or bytes that read as text), otherwise a hex dump of the start
    /// </summary>
    private static void LoadData(Memory<byte> data, string name, Result result)
    {
        if (DCX.Is(data))
            data = DCX.Decompress(data);

        var lower = name.ToLowerInvariant();
        if (lower.EndsWith(".dcx"))
            lower = lower[..^4];

        var span = data.Span;

        if (TextExtensions.Any(lower.EndsWith) || LooksLikeText(span))
        {
            var length = Math.Min(span.Length, MaxTextBytes);
            result.Kind = PreviewKind.Text;
            result.Text = Encoding.UTF8.GetString(span[..length]);

            if (span.Length > MaxTextBytes)
                result.Text += $"\n\n{LOC.Get("FILE_ItemViewer_Preview_Truncated", MaxTextBytes / 1024)}";

            return;
        }

        result.Kind = PreviewKind.Hex;
        result.Text = HexDump(span[..Math.Min(span.Length, MaxHexBytes)], span.Length);
    }

    private static bool LooksLikeText(ReadOnlySpan<byte> bytes)
    {
        var sample = bytes[..Math.Min(bytes.Length, 4096)];
        if (sample.Length == 0)
            return false;

        var printable = 0;

        foreach (var b in sample)
        {
            if (b == 0)
                return false;

            if (b is >= 0x20 and < 0x7F or (byte)'\n' or (byte)'\r' or (byte)'\t' or >= 0x80)
                printable++;
        }

        return printable >= sample.Length * 0.95;
    }

    private static string HexDump(ReadOnlySpan<byte> bytes, int totalLength)
    {
        var sb = new StringBuilder();

        for (var offset = 0; offset < bytes.Length; offset += 16)
        {
            var row = bytes.Slice(offset, Math.Min(16, bytes.Length - offset));

            sb.Append(offset.ToString("X8")).Append("  ");

            for (var i = 0; i < 16; i++)
            {
                sb.Append(i < row.Length ? row[i].ToString("X2") : "  ");
                sb.Append(i == 7 ? "  " : " ");
            }

            sb.Append(' ');

            foreach (var b in row)
                sb.Append(b is >= 0x20 and < 0x7F ? (char)b : '.');

            sb.Append('\n');
        }

        if (totalLength > bytes.Length)
            sb.Append($"\n{LOC.Get("FILE_ItemViewer_Preview_Hex_Truncated", bytes.Length, totalLength)}");

        return sb.ToString();
    }
}
