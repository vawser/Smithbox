using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActBinderView
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActBinderView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private FileDictionaryEntry TargetFileEntry;
    private bool LoadBinder = false;

    public void Display()
    {
        // File List
        ImGui.Begin("Files##TimeActFileList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.File);

        ImGui.InputText($"Search##fileContainerFilter", ref Editor.Filters._fileContainerFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("ContainerList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.File);

        if (ImGui.CollapsingHeader("Characters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            int index = 0;

            foreach (var entry in Project.TimeActData.PrimaryBank.Entries)
            {
                var file = entry.Key;
                var binder = entry.Value;

                // Ignore all of the anibnds that don't contain TAE files
                // cXXXX.anibnd.dcx is the pattern for those that do
                if(file.Filename.Length != 5)
                {
                    continue;
                }

                if (Editor.Filters.FileContainerFilter(file.Filename))
                {
                    var isSelected = false;
                    if (file.Filename == Editor.Selection.SelectedFileKey)
                    {
                        isSelected = true;
                    }

                    // File row
                    if (ImGui.Selectable($@" {file.Filename}##fileEntry{index}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        if (!LoadBinder)
                        {
                            TargetFileEntry = file;
                            LoadBinder = true;
                        }
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectFile)
                    {
                        Editor.Selection.SelectFile = false;

                        if (!LoadBinder)
                        {
                            TargetFileEntry = file;
                            LoadBinder = true;
                        }
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectFile = true;
                    }

                    if (ImGui.IsItemVisible())
                    {
                        TimeActUtils.DisplayTimeActFileAlias(Editor, file.Filename, TimeActAliasType.Character);
                    }

                    Editor.ContextMenu.ContainerMenu(isSelected, file, binder);

                    if (Editor.Selection.FocusContainer)
                    {
                        Editor.Selection.FocusContainer = false;

                        ImGui.SetScrollHereY();
                    }
                }

                index++;
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }

    public void Update()
    {
        if (LoadBinder)
        {
            Editor.Selection.ResetSelection();

            Task<bool> loadTask = Project.TimeActData.PrimaryBank.LoadTimeActBinder(TargetFileEntry);

            Task.WaitAll(loadTask);

            var targetBinder = Project.TimeActData.PrimaryBank.Entries.FirstOrDefault(e => e.Key.Filename == TargetFileEntry.Filename);

            if (targetBinder.Key != null)
            {
                Editor.Selection.FileContainerChange(targetBinder.Key, targetBinder.Value);
            }

            LoadBinder = false;
        }
    }
}
