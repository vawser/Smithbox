using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataEnflEditor
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    public MapDataEnflEditor(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Entry File Lists", "");

        EditorFilters.DisplayFramedListFilter("enflEditor_FileList",
            ref FileListFilter, ref ExactFileListFilter);
    }

    public void DisplaySourceList()
    {
        var primaryBank = Project.Handler.MapDataHandler.PrimaryBank_ENFL;

        foreach (var entry in primaryBank.EntryFileLists)
        {
            var isSelected = entry.Key == View.Selection.SelectedListDescriptor;

            if (ImGui.Selectable($"{entry.Key.Filename}", isSelected))
            {
                View.Selection.SelectedListDescriptor = entry.Key;
                View.Selection.SelectedList = entry.Value;

                var loadTask = primaryBank.LoadEntryFileList(entry.Key);
                if (loadTask.Result)
                {
                    Smithbox.Log<MapDataMsbEditor>($"Loaded entry file list: {entry.Key.Filename}");
                }
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && View.Selection.SelectListEntry)
            {
                View.Selection.SelectListEntry = false;
                View.Selection.SelectedListDescriptor = entry.Key;
                View.Selection.SelectedList = entry.Value;

                var loadTask = primaryBank.LoadEntryFileList(entry.Key);
                if (loadTask.Result)
                {
                    Smithbox.Log<MapDataMsbEditor>($"Loaded entry file list: {entry.Key.Filename}");
                }
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    View.Selection.SelectListEntry = true;
                }
            }
        }
    }

    public void Draw()
    {
        UIHelper.SimpleHeader("Current Entry File List", "");

    }
}
