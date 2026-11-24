using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.HavokEditor;

public class HavokCommandQueue
{
    private HavokEditorScreen Editor;
    private ProjectEntry Project;

    public bool DoFocus = false;
    public HavokCommandQueue(HavokEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Parse(string[] initcmd)
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (initcmd == null || initcmd.Length == 0)
            return;

        if (Editor.BehaviorMode)
        {
            // Register time act animations in a character's behavior: "behavior/register/{character}/{full ID},{full ID},..."
            // (sent by the Time Act Editor)
            if (initcmd[0] == "register" && initcmd.Length >= 3)
            {
                Register(activeView, initcmd[1], initcmd[2]);
            }

            // Show a behbnd's file: "behavior/select/{binder name}/{file name}"
            if (initcmd[0] == "select" && initcmd.Length >= 3)
            {
                Select(activeView, nameof(HavokCategoryMode.Behavior), initcmd[1], initcmd[2]);
            }

            return;
        }

        // Show a file in a binder: "hkx/select/{category}/{binder name}/{file name}"
        // (sent by the Map Editor's properties, for a collision part or navmesh entry)
        if (initcmd[0] == "select" && initcmd.Length >= 4)
        {
            Select(activeView, initcmd[1], initcmd[2], initcmd[3]);
        }
    }

    /// <summary>
    /// Selects the category, the binder of that name and the file of that name in it, as picking them in the lists.
    /// The file is matched by its name alone, as a binder's entries can be inside a folder (map collision).
    /// </summary>
    private void Select(HavokEditorView view, string categoryName, string binderName, string fileName)
    {
        if (!Enum.TryParse<HavokCategoryMode>(categoryName, out var category))
            return;

        // Behavior files are only shown by the Behavior Editor, and it shows nothing else
        if ((category is HavokCategoryMode.Behavior) != Editor.BehaviorMode)
            return;

        var binders = category switch
        {
            HavokCategoryMode.Animation => Project.Locator.HavokAnimationFiles.Entries,
            HavokCategoryMode.Behavior => Project.Locator.HavokBehaviorFiles.Entries,
            HavokCategoryMode.Character => Project.Locator.HavokCharacterFiles.Entries,
            HavokCategoryMode.Map_Collision => Project.Locator.HavokCollisionFiles.Entries,
            HavokCategoryMode.Asset_Collision => Project.Locator.HavokAssetFiles.Entries,
            HavokCategoryMode.Navmesh => Project.Locator.HavokNavmeshFiles.Entries,
            HavokCategoryMode.Cutscene => Project.Locator.HavokCutsceneFiles.Entries,
            HavokCategoryMode.Part_Collidable => Project.Locator.HavokPartFiles.Entries,
            HavokCategoryMode.Rumble => Project.Locator.HavokRumbleFiles.Entries,
            _ => null
        };

        var binder = binders?.FirstOrDefault(e => e.Filename.Equals(binderName, StringComparison.OrdinalIgnoreCase));

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Select_No_Binder", binderName));
            return;
        }

        view.Selection.ClearSelection();
        view.Selection.CategoryMode = category;
        view.Selection.RebuildBinderAliasCache();
        view.Selection.BinderFileEntry = binder;
        view.Selection.ClearFileAliasCache();
        view.BinderView.PopulateFileList(true);

        // The binder list's collision type filter would hide the binder: shown with every type instead
        if (category is HavokCategoryMode.Map_Collision)
        {
            var filter = CFG.Current.HavokEditor.HavokEditor_FileList_Collision_Type_Filter;
            var hidden = (filter == 1 && !binderName.StartsWith("l"))
                || (filter == 2 && !binderName.StartsWith("h"))
                || (filter == 3 && !binderName.StartsWith("f"));

            if (hidden)
                CFG.Current.HavokEditor.HavokEditor_FileList_Collision_Type_Filter = 0;
        }

        var files = view.HavokBank.GetBank(category)?.TryGetValue(binder, out var bankFiles) == true
            ? bankFiles.Keys
            : Enumerable.Empty<string>();

        // By the whole name, or without the extension (navmesh files are named n30_00_00_00_000100 by the Map Editor)
        var file = files.FirstOrDefault(f => FileName(f).Equals(fileName, StringComparison.OrdinalIgnoreCase))
            ?? files.FirstOrDefault(f => FileName(f).Split('.')[0].Equals(fileName, StringComparison.OrdinalIgnoreCase));

        if (file == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Select_No_File", fileName, binderName));
            return;
        }

        view.FileView.SelectFile(file);
        view.FileView.ScrollToSelection = true;
    }

    private static string FileName(string path) => System.IO.Path.GetFileName(path.Replace('\\', '/'));

    /// <summary>
    /// Selects the character's behbnd and behavior file, then opens Register in Behavior once the file is shown
    /// </summary>
    private void Register(HavokEditorView view, string character, string idList)
    {
        var ids = idList.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => long.TryParse(s, out var id) ? id : -1)
            .Where(id => id >= 0)
            .ToList();

        var binder = Project.Locator.HavokBehaviorFiles.Entries
            .FirstOrDefault(e => e.Filename.Equals(character, StringComparison.OrdinalIgnoreCase));

        if (binder == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Register_No_Binder", character));
            return;
        }

        // As picking the Behavior category, then the binder, in the lists
        view.Selection.ClearSelection();
        view.Selection.CategoryMode = HavokCategoryMode.Behavior;
        view.Selection.RebuildBinderAliasCache();
        view.Selection.BinderFileEntry = binder;
        view.Selection.ClearFileAliasCache();
        view.BinderView.PopulateFileList(true);

        var files = view.HavokBank.BehaviorBank.TryGetValue(binder, out var bankFiles)
            ? bankFiles.Keys.Where(HavokBehaviorActionsView.IsBehaviorFile).ToList()
            : new List<string>();

        if (files.Count == 0)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_Register_No_Behavior_File", character));
            return;
        }

        // The graph that holds the character's selectors: the shared enemy graph (c9997) if the binder has one,
        // else the character's own (c0000.hkx). The dialog can switch to the binder's other behavior files.
        var file = files.FirstOrDefault(f => FileStem(f).Equals("c9997", StringComparison.OrdinalIgnoreCase))
            ?? files.FirstOrDefault(f => FileStem(f).Equals(character, StringComparison.OrdinalIgnoreCase))
            ?? files[0];

        view.FileView.SelectFile(file);
        view.FileView.ScrollToSelection = true;
        view.PropertyView.BehaviorView.ActionsView.RequestRegister(character, binder, file, ids);
    }

    private static string FileStem(string path) => System.IO.Path.GetFileName(path.Replace('\\', '/')).Split('.')[0];
}
