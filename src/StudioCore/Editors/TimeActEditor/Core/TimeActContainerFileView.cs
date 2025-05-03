using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.TimeActSelectionManager;
using static StudioCore.Editors.TimeActEditor.Utils.TimeActUtils;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActContainerFileView
{
    private TimeActEditorScreen Editor;
    private TimeActSelectionManager Selection;
    private TimeActDecorator Decorator;

    public TimeActContainerFileView(TimeActEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        // File List
        ImGui.Begin("Files##TimeActFileList");
        Selection.SwitchWindowContext(TimeActEditorContext.File);

        ImGui.InputText($"Search##fileContainerFilter", ref TimeActFilters._fileContainerFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("ContainerList");
        Selection.SwitchWindowContext(TimeActEditorContext.File);

        if (ImGui.CollapsingHeader("Characters", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < Editor.Project.TimeActData.PrimaryCharacterBank.Entries.Count; i++)
            {
                var info = Editor.Project.TimeActData.PrimaryCharacterBank.Entries.ElementAt(i).Key;
                var binder = Editor.Project.TimeActData.PrimaryCharacterBank.Entries.ElementAt(i).Value;

                if (TimeActFilters.FileContainerFilter(Editor, info))
                {
                    var isSelected = false;
                    if (i == Selection.ContainerIndex)
                    {
                        isSelected = true;
                    }

                    // File row
                    if (ImGui.Selectable($@" {info.Name}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection.FileContainerChange(info, binder, i, FileContainerType.Character);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectChrContainer)
                    {
                        Selection.SelectChrContainer = false;
                        Selection.FileContainerChange(info, binder, i, FileContainerType.Character);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectChrContainer = true;
                    }

                    if (ImGui.IsItemVisible())
                    {
                        TimeActUtils.DisplayTimeActFileAlias(Editor, info.Name, TimeActAliasType.Character);
                    }

                    Selection.ContextMenu.ContainerMenu(isSelected, info.Name);

                    if (Selection.FocusContainer)
                    {
                        Selection.FocusContainer = false;

                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        var title = $"{TimeActUtils.GetObjectTitle(Editor.Project)}s";

        if (ImGui.CollapsingHeader(title))
        {
            for (int i = 0; i < Editor.Project.TimeActData.PrimaryObjectBank.Entries.Count; i++)
            {
                var info = Editor.Project.TimeActData.PrimaryObjectBank.Entries.ElementAt(i).Key;
                var binder = Editor.Project.TimeActData.PrimaryObjectBank.Entries.ElementAt(i).Value;

                if (TimeActFilters.FileContainerFilter(Editor, info))
                {
                    var isSelected = false;
                    if (i == Selection.ContainerIndex)
                    {
                        isSelected = true;
                    }

                    // File row
                    if (ImGui.Selectable($@" {info.Name}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Selection.FileContainerChange(info, binder, i, FileContainerType.Object);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Selection.SelectObjContainer)
                    {
                        Selection.SelectObjContainer = false;
                        Selection.FileContainerChange(info, binder, i, FileContainerType.Object);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Selection.SelectObjContainer = true;
                    }

                    if (ImGui.IsItemVisible())
                    {
                        TimeActUtils.DisplayTimeActFileAlias(Editor, info.Name, TimeActAliasType.Asset);
                    }

                    Selection.ContextMenu.ContainerMenu(isSelected, info.Name);

                    if (Selection.FocusContainer)
                    {
                        Selection.FocusContainer = false;

                        ImGui.SetScrollHereY();
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
