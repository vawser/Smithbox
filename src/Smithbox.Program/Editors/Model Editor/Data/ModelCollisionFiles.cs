using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// One collision file of a model, as the Model Editor's Collision tool edits it: its document, where it is stored,
/// and whether it has been changed (or made) since it was read, so the model's save writes it.
/// </summary>
public sealed class ModelCollisionFile
{
    public HavokCollisionType Type { get; init; }

    /// <summary>The file's name without extensions, as its binder names it: aeg020_000_h, or the model's own name.</summary>
    public string EntryName { get; init; }

    /// <summary>The collision binder beside the model binder (geomhkxbnd), or null when the file is in the model binder.</summary>
    public string BinderPath { get; init; }

    public Havok.HavokDocument Document { get; init; }

    /// <summary>Changed since it was read or saved, so the next save writes it.</summary>
    public bool IsModified { get; set; }

    /// <summary>Made by the Collision tool, so the next save adds it to its binder (making the binder if needed).</summary>
    public bool IsNew { get; set; }
}

/// <summary>
/// The collision files of a model that the Collision tool reads and edits, by layer, read when first asked for.
/// <para>
/// Assets (a geombnd model binder) keep each layer in a binder of its own beside the model binder, one per collision
/// type: aeg020_000_h.geomhkxbnd.dcx holds aeg020_000_h, the high collision. Other models keep a single collision
/// file in their own binder, named as the model (the one the Model Editor already shows). Which applies follows the
/// model's binder, not the game.
/// </para>
/// </summary>
public sealed class ModelCollisionFiles
{
    private readonly ModelWrapper Wrapper;

    private readonly Dictionary<HavokCollisionType, ModelCollisionFile> Files = new();

    // Layers looked for and not found (or not readable), so they aren't read again every frame
    private readonly HashSet<HavokCollisionType> Missing = new();

    public ModelCollisionFiles(ModelWrapper wrapper)
    {
        Wrapper = wrapper;
    }

    private ProjectEntry Project => Wrapper.Parent.Project;

    /// <summary>Whether the model's collision is in binders of its own beside the model binder, one per layer.</summary>
    public bool UsesLayerBinders => Wrapper.Parent.Path.EndsWith(".geombnd.dcx", StringComparison.OrdinalIgnoreCase);

    /// <summary>The layers the model can have: high and low with layer binders, else its one collision file.</summary>
    public IReadOnlyList<HavokCollisionType> Layers => UsesLayerBinders
        ? [HavokCollisionType.High, HavokCollisionType.Low]
        : [HavokCollisionType.Low];

    private static string Suffix(HavokCollisionType type) => type is HavokCollisionType.High ? "h" : "l";

    private static HavokCollisionType Other(HavokCollisionType type) =>
        type is HavokCollisionType.High ? HavokCollisionType.Low : HavokCollisionType.High;

    /// <summary>The binder a layer is kept in: aeg020_000_h.geomhkxbnd.dcx beside aeg020_000.geombnd.dcx.</summary>
    public string GetBinderPath(HavokCollisionType type)
    {
        var path = Wrapper.Parent.Path;
        var folder = path.Substring(0, path.LastIndexOf('/') + 1);

        return $"{folder}{Wrapper.Parent.Name}_{Suffix(type)}.geomhkxbnd.dcx";
    }

    /// <summary>The name a layer's file has in its binder.</summary>
    public string GetEntryName(HavokCollisionType type) => UsesLayerBinders ? $"{Wrapper.Name}_{Suffix(type)}" : Wrapper.Name;

    /// <summary>The layer's file, read if it hasn't been. Null if the model has none (or it can't be read).</summary>
    public ModelCollisionFile Get(HavokCollisionType type)
    {
        if (Files.TryGetValue(type, out var file))
            return file;

        if (Missing.Contains(type) || !Layers.Contains(type))
            return null;

        file = UsesLayerBinders ? ReadLayerFile(type) : ReadEmbeddedFile();

        if (file == null)
            Missing.Add(type);
        else
            Files[type] = file;

        return file;
    }

    /// <summary>The files read so far.</summary>
    public IEnumerable<ModelCollisionFile> Loaded => Files.Values;

    public bool HasModified => Files.Values.Any(f => f.IsModified);

    /// <summary>Whether the layer's binder exists, so a new file can be added to it rather than a binder made.</summary>
    public bool LayerBinderExists(HavokCollisionType type) =>
        UsesLayerBinders && Wrapper.Parent.TargetFS.FileExists(GetBinderPath(type));

    /// <summary>Adds a file made by the Collision tool (or one taken back by an undo, with null).</summary>
    public void SetNewFile(HavokCollisionType type, ModelCollisionFile file)
    {
        if (file == null)
        {
            if (Files.TryGetValue(type, out var existing) && existing.IsNew)
            {
                Files.Remove(type);
                Missing.Add(type);
            }

            return;
        }

        Files[type] = file;
        Missing.Remove(type);
    }

    /// <summary>Forgets everything read, for when the model is read from storage again.</summary>
    public void Clear()
    {
        Files.Clear();
        Missing.Clear();
    }

    private ModelCollisionFile ReadEmbeddedFile()
    {
        // The collision the model binder holds, as the model was read
        if (Wrapper.HKX_Document == null || Wrapper.HKX_BinderName != null)
            return null;

        return new ModelCollisionFile
        {
            Type = HavokCollisionType.Low,
            EntryName = Wrapper.Name,
            BinderPath = null,
            Document = Wrapper.HKX_Document
        };
    }

    private ModelCollisionFile ReadLayerFile(HavokCollisionType type)
    {
        var binderName = $"{Wrapper.Parent.Name}_{Suffix(type)}";
        var entryName = GetEntryName(type);

        // The layer the model was shown with is the same document, so the hierarchy and the tool agree
        if (Wrapper.HKX_Document != null && string.Equals(Wrapper.HKX_BinderName, binderName, StringComparison.OrdinalIgnoreCase)
            && string.Equals(Wrapper.HKX_Name, entryName, StringComparison.OrdinalIgnoreCase))
        {
            return new ModelCollisionFile { Type = type, EntryName = entryName, BinderPath = GetBinderPath(type), Document = Wrapper.HKX_Document };
        }

        var binderPath = GetBinderPath(type);
        var fs = Wrapper.Parent.TargetFS;

        if (!fs.FileExists(binderPath))
            return null;

        try
        {
            var data = fs.ReadFile(binderPath);

            if (data == null)
                return null;

            using var binder = BND4.Read(data.Value);

            var entry = binder.Files.FirstOrDefault(f =>
                string.Equals(FilePathUtils.GetPureFilename(f.Name), entryName, StringComparison.OrdinalIgnoreCase));

            if (entry == null)
                return null;

            var document = ReadDocument(entry.Bytes);

            return document == null ? null : new ModelCollisionFile
            {
                Type = type,
                EntryName = entryName,
                BinderPath = binderPath,
                Document = document
            };
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Read_Failed", binderPath), e);
            return null;
        }
    }

    /// <summary>A collision file's document, through the unified Havok library. Null if the game has no profile.</summary>
    public Havok.HavokDocument ReadDocument(Memory<byte> data)
    {
        var profile = Havok.HavokProfile.FromGame(Project.Descriptor.ProjectType.ToString());

        if (profile == null)
            return null;

        var bytes = DCX.Is(data) ? DCX.Decompress(data).ToArray() : data.ToArray();

        return Havok.Files.HavokFile.Read(bytes, profile);
    }

    /// <summary>
    /// A fresh copy of a layer's document as stored, to base a new layer on (its class set and settings). Null if the
    /// layer has no file.
    /// </summary>
    public Havok.HavokDocument ReadTemplateDocument(HavokCollisionType type)
    {
        if (!UsesLayerBinders)
            return null;

        try
        {
            var binderPath = GetBinderPath(type);
            var data = Wrapper.Parent.TargetFS.FileExists(binderPath) ? Wrapper.Parent.TargetFS.ReadFile(binderPath) : null;

            if (data == null)
                return null;

            using var binder = BND4.Read(data.Value);
            var entry = binder.Files.FirstOrDefault(f =>
                string.Equals(FilePathUtils.GetPureFilename(f.Name), GetEntryName(type), StringComparison.OrdinalIgnoreCase));

            return entry == null ? null : ReadDocument(entry.Bytes);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Read_Failed", GetBinderPath(type)), e);
            return null;
        }
    }

    /// <summary>
    /// Writes a changed file into its bytes, keeping the DCX compression the old bytes had. Used for the file in the
    /// model binder, which the model's own save writes.
    /// </summary>
    public static byte[] WriteDocument(Havok.HavokDocument document, Memory<byte> oldBytes)
    {
        var written = Havok.Files.HavokFile.Write(document);

        if (!oldBytes.IsEmpty && DCX.Is(oldBytes))
        {
            DCX.Decompress(oldBytes, out DCX.Type compression);
            return DCX.Compress(written, compression);
        }

        return written;
    }

    /// <summary>The model binder's collision file, if it has changed and should be written by the model's save.</summary>
    public ModelCollisionFile GetEmbeddedToWrite() =>
        Files.Values.FirstOrDefault(f => f.BinderPath == null && f.IsModified);

    /// <summary>
    /// Writes the changed layer files into their binders in the project: into the file of the same name, or as a new
    /// entry named and flagged as the model's other layer, in a binder made as a copy of the other layer's when the
    /// layer had none. False if anything failed.
    /// </summary>
    public bool SaveLayerBinders()
    {
        var success = true;

        foreach (var file in Files.Values.Where(f => f.IsModified && f.BinderPath != null).ToList())
        {
            try
            {
                if (SaveLayerFile(file))
                {
                    file.IsModified = false;
                    file.IsNew = false;
                    Smithbox.Log(this, LOC.Get("MODEL_CollisionTool_Saved", file.EntryName, file.BinderPath));
                }
                else
                {
                    success = false;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Save_Failed", file.EntryName, file.BinderPath), e);
                success = false;
            }
        }

        return success;
    }

    private bool SaveLayerFile(ModelCollisionFile file)
    {
        var fs = Wrapper.Parent.TargetFS;
        var otherPath = GetBinderPath(Other(file.Type));

        BND4 binder;

        if (fs.FileExists(file.BinderPath))
        {
            binder = BND4.Read(fs.ReadFile(file.BinderPath).Value);
        }
        else
        {
            // No binder for this layer: one like the other layer's, holding only this file
            if (!fs.FileExists(otherPath))
            {
                Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Save_No_Template", file.EntryName));
                return false;
            }

            var other = BND4.Read(fs.ReadFile(otherPath).Value);
            var template = FindEntry(other, GetEntryName(Other(file.Type))) ?? other.Files.FirstOrDefault();

            if (template == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Save_No_Template", file.EntryName));
                return false;
            }

            other.Files.Clear();
            other.Files.Add(MakeEntry(template, template.ID, file));
            binder = other;
        }

        // The new binder made above already holds the file
        var entry = FindEntry(binder, file.EntryName);

        if (entry != null)
        {
            if (fs.FileExists(file.BinderPath))
                entry.Bytes = WriteDocument(file.Document, entry.Bytes);
        }
        else
        {
            // A new entry in an existing binder: named and flagged as the model's other layer, or the binder's first
            var otherEntry = fs.FileExists(otherPath) ? FindEntry(BND4.Read(fs.ReadFile(otherPath).Value), GetEntryName(Other(file.Type))) : null;
            var template = otherEntry ?? binder.Files.FirstOrDefault();

            if (template == null)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_CollisionTool_Save_No_Template", file.EntryName));
                return false;
            }

            var id = binder.Files.Count == 0 ? template.ID : binder.Files.Max(f => f.ID) + 1;
            binder.Files.Add(MakeEntry(template, id, file));
        }

        Project.VFS.ProjectFS.WriteFile(file.BinderPath, binder.Write());
        return true;
    }

    private static BinderFile FindEntry(IBinder binder, string entryName) => binder.Files.FirstOrDefault(f =>
        string.Equals(FilePathUtils.GetPureFilename(f.Name), entryName, StringComparison.OrdinalIgnoreCase));

    /// <summary>A new entry for a file, named, flagged and compressed as the template entry (the other layer's).</summary>
    private BinderFile MakeEntry(BinderFile template, int id, ModelCollisionFile file)
    {
        var templateName = FilePathUtils.GetPureFilename(template.Name);
        var name = ReplaceLast(template.Name, templateName, file.EntryName);

        return new BinderFile(template.Flags, id, name, WriteDocument(file.Document, template.Bytes))
        {
            CompressionType = template.CompressionType
        };
    }

    private static string ReplaceLast(string text, string find, string replace)
    {
        var index = text.LastIndexOf(find, StringComparison.OrdinalIgnoreCase);

        return index < 0 ? text : text.Substring(0, index) + replace + text.Substring(index + find.Length);
    }
}
