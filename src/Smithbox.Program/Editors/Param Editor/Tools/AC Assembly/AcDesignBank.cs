using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The project's AC6 design binder (<c>param/asmparam/asmparam.designbnd.dcx</c>). Read through the project VFS (the
/// project's copy if it has one, else the game's), and written to the project when the Param Editor saves.
/// Edits are undoable: each one is an <see cref="AcDesignEditAction"/> holding the file's bytes before and after.
/// </summary>
public class AcDesignBank
{
    public const string BinderPath = "/param/asmparam/asmparam.designbnd.dcx";

    /// <summary>
    /// The folder the vanilla binder's entries are named under
    /// </summary>
    private const string EntryFolder = @"W:\FNR\data\Target\INTERROOT_win64\param\AsmParam\";

    private readonly ProjectEntry Project;

    private BND4 Bnd;
    private bool Loaded;

    public string LoadError { get; private set; }

    public bool IsDirty { get; private set; }

    /// <summary>
    /// Goes up whenever a design changes, so views can refresh what they show
    /// </summary>
    public int Version { get; private set; }

    private readonly Dictionary<int, BinderFile> FilesById = new();
    private readonly Dictionary<int, AcDesign> Parsed = new();
    private readonly Dictionary<int, string> ParseErrors = new();

    public AcDesignBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool EnsureLoaded()
    {
        if (Loaded)
            return Bnd != null;

        Loaded = true;

        try
        {
            var data = Project.VFS.FS.ReadFile(BinderPath);

            if (data == null)
            {
                LoadError = LOC.Get("PARAM_AcAssembly_Error_No_Binder", BinderPath);
                return false;
            }

            Bnd = BND4.Read(data.Value);
            Reindex();
        }
        catch (Exception e)
        {
            Bnd = null;
            LoadError = LOC.Get("PARAM_AcAssembly_Error_Read_Binder", e.Message);
            Smithbox.LogError(this, LoadError, e);
        }

        return Bnd != null;
    }

    private void Reindex()
    {
        FilesById.Clear();
        Parsed.Clear();
        ParseErrors.Clear();

        foreach (var file in Bnd.Files)
        {
            if (TryGetId(file, out var id))
                FilesById.TryAdd(id, file);
        }
    }

    /// <summary>
    /// The design's ID, from the entry name: "09990004.design" is 9990004
    /// </summary>
    private static bool TryGetId(BinderFile file, out int id)
    {
        var name = Path.GetFileNameWithoutExtension(file.Name?.Replace('\\', '/') ?? "");
        return int.TryParse(name, out id);
    }

    public IEnumerable<int> DesignIds => FilesById.Keys.OrderBy(k => k);

    public bool HasDesign(int id) => FilesById.ContainsKey(id);

    public string GetEntryName(int id)
    {
        return FilesById.TryGetValue(id, out var file) ? Path.GetFileName(file.Name.Replace('\\', '/')) : null;
    }

    public byte[] GetBytes(int id)
    {
        return FilesById.TryGetValue(id, out var file) ? file.Bytes.ToArray() : null;
    }

    /// <summary>
    /// The parsed design, or null (with <paramref name="error"/> set when the file exists but can't be read)
    /// </summary>
    public AcDesign GetDesign(int id, out string error)
    {
        error = null;

        if (!FilesById.TryGetValue(id, out var file))
            return null;

        if (Parsed.TryGetValue(id, out var design))
            return design;

        if (ParseErrors.TryGetValue(id, out error))
            return null;

        if (AcDesign.TryRead(file.Bytes.ToArray(), out design, out error))
        {
            Parsed[id] = design;
            return design;
        }

        ParseErrors[id] = error;
        return null;
    }

    /// <summary>
    /// Sets the design's file bytes, adding the entry when it doesn't exist, or removes it when bytes is null.
    /// Called by <see cref="AcDesignEditAction"/>; use the action so the change can be undone.
    /// </summary>
    internal void SetBytes(int id, byte[] bytes)
    {
        if (Bnd == null)
            return;

        if (bytes == null)
        {
            if (FilesById.Remove(id, out var removed))
                Bnd.Files.Remove(removed);
        }
        else if (FilesById.TryGetValue(id, out var file))
        {
            file.Bytes = bytes;
        }
        else
        {
            var name = $"{EntryFolder}{id}.design";
            var entry = new BinderFile(Bnd.Files.FirstOrDefault()?.Flags ?? Binder.FileFlags.Flag1, -1, name, bytes);

            // Kept in name order, as in the vanilla binder, with the IDs renumbered to match
            var index = Bnd.Files.FindIndex(f => string.CompareOrdinal(f.Name, name) > 0);
            Bnd.Files.Insert(index < 0 ? Bnd.Files.Count : index, entry);

            for (var i = 0; i < Bnd.Files.Count; i++)
                Bnd.Files[i].ID = i;

            FilesById[id] = entry;
        }

        Parsed.Remove(id);
        ParseErrors.Remove(id);

        IsDirty = true;
        Version++;
    }

    public bool Save()
    {
        if (Bnd == null || !IsDirty)
            return true;

        try
        {
            Project.VFS.ProjectFS.WriteFile(BinderPath, Bnd.Write());
            IsDirty = false;

            Smithbox.Log(this, LOC.Get("PARAM_AcAssembly_Saved", BinderPath));
            return true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_AcAssembly_Error_Write_Binder", e.Message), e);
            return false;
        }
    }
}

/// <summary>
/// One undoable change to a design file: its whole bytes before and after. A null side means the file didn't exist
/// (so undoing a creation removes it again).
/// </summary>
public class AcDesignEditAction : EditorAction
{
    private readonly AcDesignBank Bank;
    private readonly int Id;
    private readonly byte[] Before;
    private readonly byte[] After;

    public AcDesignEditAction(AcDesignBank bank, int id, byte[] before, byte[] after, string displayName)
    {
        Bank = bank;
        Id = id;
        Before = before;
        After = after;
        DisplayName = displayName;
    }

    public override ActionEvent Execute()
    {
        Bank.SetBytes(Id, After);
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bank.SetBytes(Id, Before);
        return ActionEvent.NoEvent;
    }
}
