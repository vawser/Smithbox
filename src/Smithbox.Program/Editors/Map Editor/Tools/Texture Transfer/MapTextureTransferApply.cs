using Andre.IO.VFS;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace StudioCore.Editors.MapEditor;

/// <summary>One texture to copy: its name and the binder to copy it from</summary>
public record TextureTransferRequest(string Name, string Source);

public class TextureTransferApplyResult
{
    public int Added;
    public int LowDetailAdded;
    public List<string> Skipped = new();
    public List<string> Errors = new();

    /// <summary>The target containers written, for the bank to refresh the scenes (main thread)</summary>
    public List<KeyValuePair<FileDictionaryEntry, BinderContents>> Saved = new();
}

/// <summary>
/// Copies map textures into the target binders, through the Texture Editor's bank so it sees the new files. A source
/// binder file is copied byte for byte (its TPF and compression untouched); it is named in the target binder's style
/// (DS1R "\name.tpf.dcx", DS2 "mAA_BB_CC_DD\name.tpf.dcx"), takes the next ID, and goes into the target binder that is
/// smallest at that point, which keeps the four area binders about even as the game's own split does. A texture's
/// low detail twin (name_l) is copied with it when the source has one. Runs on a background task.
/// </summary>
public static class MapTextureTransferApply
{
    public static TextureTransferApplyResult Run(ProjectEntry project, TextureBank bank, List<FileDictionaryEntry> targetEntries,
        List<TextureTransferRequest> requests, CancellationToken token, Action<int, int, string> progress)
    {
        var result = new TextureTransferApplyResult();
        var fs = project.VFS.FS;

        // The target binders, loaded into the bank (the Texture Editor may have them already, with its edits)
        var targets = new List<(FileDictionaryEntry Entry, BinderContents Contents)>();

        foreach (var entry in targetEntries)
        {
            token.ThrowIfCancellationRequested();
            progress(0, requests.Count, entry.Filename);

            if (!bank.PackedEntries.TryGetValue(entry, out var contents) || contents == null)
            {
                bank.LoadPackedTextureBinder(entry).Wait(token);
                bank.PackedEntries.TryGetValue(entry, out contents);
            }

            if (contents?.Binder == null)
            {
                result.Errors.Add(LOC.Get("MAP_TextureTransfer_Error_Target", entry.Path));
                continue;
            }

            targets.Add((entry, contents));
        }

        if (targets.Count == 0)
            return result;

        // Every name the targets hold, so nothing is added twice (also names added by this run)
        var held = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (_, contents) in targets)
            foreach (var file in contents.Binder.Files)
                held.Add(MapTextureTransferProfile.Stem(file.Name));

        var sizes = targets.ToDictionary(t => t.Contents, t => t.Contents.Binder.Files.Sum(f => (long)f.Bytes.Length));
        var changed = new HashSet<BinderContents>();
        var done = 0;

        foreach (var group in requests.GroupBy(r => r.Source, StringComparer.OrdinalIgnoreCase))
        {
            token.ThrowIfCancellationRequested();

            IBinder source;
            try
            {
                source = ReadBinder(fs, group.Key);
            }
            catch (Exception e)
            {
                foreach (var r in group)
                    result.Errors.Add(LOC.Get("MAP_TextureTransfer_Error_Source", r.Name, group.Key, e.Message));
                continue;
            }

            if (source == null)
            {
                foreach (var r in group)
                    result.Errors.Add(LOC.Get("MAP_TextureTransfer_Error_Source", r.Name, group.Key, "-"));
                continue;
            }

            var byStem = new Dictionary<string, BinderFile>(StringComparer.OrdinalIgnoreCase);
            foreach (var file in source.Files)
                byStem.TryAdd(MapTextureTransferProfile.Stem(file.Name), file);

            foreach (var request in group)
            {
                token.ThrowIfCancellationRequested();
                progress(done++, requests.Count, request.Name);

                if (held.Contains(request.Name))
                {
                    result.Skipped.Add(request.Name);
                    continue;
                }

                if (!byStem.TryGetValue(request.Name, out var file))
                {
                    result.Errors.Add(LOC.Get("MAP_TextureTransfer_Error_Not_In_Source", request.Name, group.Key));
                    continue;
                }

                if (!Add(targets, sizes, changed, held, file, result))
                    continue;

                result.Added++;

                // The low detail twin the area binders keep beside it
                var low = MapTextureTransferProfile.LowDetailStem(request.Name);
                if (!held.Contains(low) && byStem.TryGetValue(low, out var lowFile) && Add(targets, sizes, changed, held, lowFile, result))
                    result.LowDetailAdded++;
            }
        }

        // Written at once: the copy is a file operation, like the Texture Editor's Copy to Project
        foreach (var (entry, contents) in targets.Where(t => changed.Contains(t.Contents)))
        {
            token.ThrowIfCancellationRequested();
            progress(requests.Count, requests.Count, entry.Filename);

            if (contents.Save(project, entry))
                result.Saved.Add(new(entry, contents));
            else
                result.Errors.Add(LOC.Get("MAP_TextureTransfer_Error_Save", entry.Path));
        }

        return result;
    }

    private static bool Add(List<(FileDictionaryEntry Entry, BinderContents Contents)> targets, Dictionary<BinderContents, long> sizes,
        HashSet<BinderContents> changed, HashSet<string> held, BinderFile sourceFile, TextureTransferApplyResult result)
    {
        var bytes = sourceFile.Bytes.ToArray();

        TPF tpf;
        try
        {
            tpf = TPF.Read(bytes);
        }
        catch (Exception e)
        {
            result.Errors.Add(LOC.Get("MAP_TextureTransfer_Error_Tpf", sourceFile.Name, e.Message));
            return false;
        }

        var target = targets.OrderBy(t => sizes[t.Contents]).First().Contents;
        var like = target.Binder.Files.FirstOrDefault();

        // The source's own spelling of the name, in the target's folder and extension style
        var (_, sourceStem, sourceExtensions) = TpfCommands.SplitName(sourceFile.Name);
        var (folder, _, extensions) = like != null ? TpfCommands.SplitName(like.Name) : ("", "", sourceExtensions);
        var id = target.Binder.Files.Count > 0 ? target.Binder.Files.Max(f => f.ID) + 1 : 0;

        var file = new BinderFile(like?.Flags ?? sourceFile.Flags, id, $"{folder}{sourceStem}{extensions}", bytes)
        {
            CompressionType = sourceFile.CompressionType,
        };

        // Not marked modified: the bank writes its bytes as they are, so the copy is exact
        target.Binder.Files.Add(file);
        target.Files[file] = tpf;
        target.StructureModified = true;

        sizes[target] += bytes.Length;
        changed.Add(target);
        held.Add(MapTextureTransferProfile.Stem(file.Name));
        return true;
    }

    private static IBinder ReadBinder(VirtualFileSystem fs, string bhdPath)
    {
        var bdtPath = BinderContents.GetBdtPath(bhdPath);

        if (!fs.FileExists(bhdPath) || !fs.FileExists(bdtPath))
            return null;

        var bhd = fs.ReadFile(bhdPath);
        var bdt = fs.ReadFile(bdtPath);

        if (bhd == null || bdt == null)
            return null;

        return bhd.Value.Span[3] == '3' ? BXF3.Read(bhd.Value, bdt.Value) : BXF4.Read(bhd.Value, bdt.Value);
    }
}
