using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// Container and clipboard commands of the Texture Editor (plan, T3): TPFs added to, duplicated in, renamed in and
/// removed from the selected binder, and textures copied and pasted between PC TPFs. Every command is undoable.
/// </summary>
public class TpfCommands
{
    private readonly TexEditorView View;
    private readonly ProjectEntry Project;

    public TpfCommands(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    #region TPF names
    /// <summary>
    /// A binder file name split into its folder part, its stem (up to the first dot) and its extensions:
    /// "N:\GR\...\c2010_h.tpf" → ("N:\GR\...\", "c2010_h", ".tpf").
    /// </summary>
    public static (string Folder, string Stem, string Extensions) SplitName(string name)
    {
        var slash = Math.Max(name.LastIndexOf('\\'), name.LastIndexOf('/'));
        var folder = slash >= 0 ? name[..(slash + 1)] : "";
        var file = slash >= 0 ? name[(slash + 1)..] : name;
        var dot = file.IndexOf('.');
        return dot < 0 ? (folder, file, "") : (folder, file[..dot], file[dot..]);
    }

    /// <summary>Why a TPF stem can't be used in this container, or null.</summary>
    public static string ValidateTpfStem(BinderContents container, string stem, BinderFile except = null)
    {
        if (string.IsNullOrWhiteSpace(stem))
            return LOC.Get("TEXVIEW_TpfCommands_Name_Empty");

        if (stem.IndexOfAny(['\\', '/', ':', '*', '?', '"', '<', '>', '|', '.']) >= 0)
            return LOC.Get("TEXVIEW_TpfCommands_Name_Invalid");

        if (container.Binder.Files.Any(f => f != except && SplitName(f.Name).Stem.Equals(stem, StringComparison.OrdinalIgnoreCase)))
            return LOC.Get("TEXVIEW_TpfCommands_Name_Taken", stem);

        return null;
    }

    private static string UniqueStem(BinderContents container, string stem)
    {
        if (ValidateTpfStem(container, stem) == null)
            return stem;

        for (int i = 1; ; i++)
        {
            var candidate = $"{stem}_{i}";
            if (ValidateTpfStem(container, candidate) == null)
                return candidate;
        }
    }

    /// <summary>A new binder file next to an existing one: same folder, extensions, flags and compression, next ID.</summary>
    private static BinderFile NewBinderFile(BinderContents container, BinderFile like, string stem)
    {
        var (folder, _, extensions) = like != null ? SplitName(like.Name) : ("", "", ".tpf");
        var ids = container.Binder.Files.Select(f => f.ID).ToList();
        var id = ids.Count > 0 && ids.Max() >= 0 ? ids.Max() + 1 : -1;

        return new BinderFile(like?.Flags ?? Binder.FileFlags.Flag1, id, $"{folder}{stem}{extensions}", Array.Empty<byte>())
        {
            CompressionType = like?.CompressionType ?? DCX.Type.None,
        };
    }
    #endregion

    #region TPF commands
    /// <summary>Adds an empty TPF made like the selected (or first) one, after it.</summary>
    public void AddEmptyTpf()
    {
        var container = View.GetEditableContainer();
        if (container == null)
            return;

        var like = View.Selection.SelectedTpfBinderFile ?? container.OrderedFiles().FirstOrDefault();
        var likeTpf = like != null ? container.Files[like] : null;

        var tpf = new TPF();
        if (likeTpf != null)
        {
            tpf.Platform = likeTpf.Platform;
            tpf.Encoding = likeTpf.Encoding;
            tpf.Flag2 = likeTpf.Flag2;
            tpf.Compression = likeTpf.Compression;

            // The game's packing rules, without the original file's pad bytes
            if (likeTpf.Layout != null)
            {
                tpf.Layout = new TPF.TpfLayout
                {
                    FirstAlign = likeTpf.Layout.FirstGap == null ? likeTpf.Layout.FirstAlign : 1,
                    DataAlign = likeTpf.Layout.DataAlign,
                    TrailAlign = likeTpf.Layout.Trailing == null ? likeTpf.Layout.TrailAlign : 1,
                    SizeRule = likeTpf.Layout.SizeRule,
                };
            }
        }

        var stem = UniqueStem(container, like != null ? $"{SplitName(like.Name).Stem}_new" : "new");
        var file = NewBinderFile(container, like, stem);

        var index = like != null ? container.Binder.Files.IndexOf(like) + 1 : container.Binder.Files.Count;
        View.ActionManager.ExecuteAction(new TpfInsertRemoveAction(View, container, file, tpf, index, true));
    }

    /// <summary>Adds a copy of a TPF (all its textures) after it, named {stem}_copy.</summary>
    public void DuplicateTpf(BinderFile file)
    {
        var container = View.GetEditableContainer();
        if (container == null || file == null || !container.Files.TryGetValue(file, out var tpf))
            return;

        // Written and read back: a deep copy with the same layout rules
        var copy = TPF.Read(tpf.Write(DCX.Type.None));
        copy.Compression = tpf.Compression;

        var newFile = NewBinderFile(container, file, UniqueStem(container, $"{SplitName(file.Name).Stem}_copy"));
        View.ActionManager.ExecuteAction(new TpfInsertRemoveAction(View, container, newFile, copy, container.Binder.Files.IndexOf(file) + 1, true));
    }

    public void DeleteTpf(BinderFile file)
    {
        var container = View.GetEditableContainer();
        if (container == null || file == null || !container.Files.TryGetValue(file, out var tpf))
            return;

        View.ActionManager.ExecuteAction(new TpfInsertRemoveAction(View, container, file, tpf, container.Binder.Files.IndexOf(file), false));
    }

    public void RenameTpf(BinderFile file)
    {
        var container = View.GetEditableContainer();
        if (container == null || file == null)
            return;

        var (folder, stem, extensions) = SplitName(file.Name);

        View.NamePrompt.Open(LOC.Get("TEXVIEW_TpfCommands_Rename_Title"), LOC.Get("TEXVIEW_TpfCommands_Rename_Hint", file.Name), stem,
            s => s == stem ? null : ValidateTpfStem(container, s, file),
            s =>
            {
                if (s != stem)
                    View.ActionManager.ExecuteAction(new TpfRenameAction(View, container, file, $"{folder}{s}{extensions}"));
            });
    }
    #endregion

    #region Texture clipboard
    /// <summary>Copies the selected texture to the editor's texture clipboard (shared by all views).</summary>
    public void CopyTexture(TPF tpf, TPF.Texture tex)
    {
        if (tpf == null || tex == null || !TextureFormats.IsEditable(tpf))
            return;

        View.Editor.TextureClipboard = [TextureState.Capture(tex).Clone()];
        Smithbox.Log(this, LOC.Get("TEXVIEW_TpfCommands_Log_Copied", tex.Name));
    }

    public bool CanPaste => View.Editor.TextureClipboard is { Count: > 0 } && View.GetEditTarget() != null;

    /// <summary>Pastes the clipboard's textures into the selected TPF, after the selected texture.</summary>
    public void PasteTextures()
    {
        var target = View.GetEditTarget();
        if (target == null || !CanPaste)
            return;

        var used = new HashSet<string>(target.Tpf.Textures.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
        var index = View.Selection.SelectedTexture != null ? target.Tpf.Textures.IndexOf(View.Selection.SelectedTexture) + 1 : target.Tpf.Textures.Count;
        var actions = new List<EditorAction>();

        foreach (var state in View.Editor.TextureClipboard)
        {
            var copy = state.Clone();

            var name = copy.Name;
            for (int n = 1; used.Contains(name); n++)
                name = $"{copy.Name}_{n}";
            used.Add(name);
            copy.Name = name;

            actions.Add(new TextureInsertRemoveAction(View, target, copy.CreateTexture(target.Tpf.Platform), index++, true));
        }

        View.ActionManager.ExecuteAction(actions.Count == 1 ? actions[0] : new CompoundAction(actions));
    }
    #endregion
}
