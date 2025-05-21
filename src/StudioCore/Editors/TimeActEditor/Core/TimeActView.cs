using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActView
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin("Time Acts##TimeActList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.TimeAct);

        if (!Editor.Selection.HasSelectedBinder())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActFilter", ref Editor.Filters._timeActFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("TimeActList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.TimeAct);

        if (Editor.Selection.SelectedBinder != null)
        {
            for (int i = 0; i < Editor.Selection.SelectedBinder.Files.Count; i++)
            {
                var curFile = Editor.Selection.SelectedBinder.Files.ElementAt(i);
                var binderFile = curFile.Key;
                var taeEntry = curFile.Value;

                if (Editor.Filters.TimeActFilter(binderFile.Name, taeEntry))
                {
                    var isSelected = false;

                    if (Editor.Selection.CurrentTimeActKey == binderFile.Name ||
                        Editor.Selection.IsTimeActSelected(i))
                    {
                        isSelected = true;
                    }

                    var displayName = Path.GetFileName(binderFile.Name);

                    // Time Act row
                    if (ImGui.Selectable($@"{displayName}##TimeAct{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Editor.Selection.TimeActChange(binderFile.Name, taeEntry, i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectTimeAct)
                    {
                        Editor.Selection.SelectTimeAct = false;
                        Editor.Selection.TimeActChange(binderFile.Name, taeEntry, i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectTimeAct = true;
                    }

                    if (ImGui.IsItemVisible())
                    {
                        if (CFG.Current.TimeActEditor_DisplayTimeActRow_AliasInfo)
                            TimeActUtils.DisplayTimeActAlias(Editor, binderFile.Name, taeEntry.ID);
                    }

                    Editor.ContextMenu.TimeActMenu(isSelected, taeEntry);

                    if (Editor.Selection.FocusTimeAct)
                    {
                        Editor.Selection.FocusTimeAct = false;

                        ImGui.SetScrollHereY();
                    }
                }

            }
        }
        else
        {
            ImGui.Text("Time Act files haven't been loaded yet.");
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
