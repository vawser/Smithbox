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

        // Register time act animations in a character's behavior: "hkx/register/{character}/{full ID},{full ID},..."
        // (sent by the Time Act Editor)
        if (initcmd[0] == "register" && initcmd.Length >= 3)
        {
            Register(activeView, initcmd[1], initcmd[2]);
        }
    }

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
        view.PropertyView.BehaviorView.ActionsView.RequestRegister(character, binder, file, ids);
    }

    private static string FileStem(string path) => System.IO.Path.GetFileName(path.Replace('\\', '/')).Split('.')[0];
}
