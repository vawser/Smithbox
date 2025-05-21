using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActAnimationView
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActAnimationView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin("Animations##TimeActAnimationList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.Animation);

        if (!Editor.Selection.HasSelectedTimeAct())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActAnimationFilter", ref Editor.Filters._timeActAnimationFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("AnimationList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.Animation);

        if (Editor.Selection.CurrentTimeAct != null)
        {
            var timeAct = Editor.Selection.CurrentTimeAct;

            for (int i = 0; i < timeAct.Animations.Count; i++)
            {
                TAE.Animation entry = timeAct.Animations[i];

                if (Editor.Filters.TimeActAnimationFilter(entry))
                {
                    var isSelected = false;
                    if (i == Editor.Selection.CurrentTimeActAnimationIndex ||
                        Editor.Selection.IsAnimationSelected(i))
                    {
                        isSelected = true;
                    }

                    var displayName = $"{entry.ID}";
                    if (CFG.Current.TimeActEditor_DisplayAnimFileName)
                    {
                        displayName = $"{entry.ID} {entry.AnimFileName}";
                    }

                    // Animation row
                    if (ImGui.Selectable($@" {displayName}##taeAnim{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                    {
                        Editor.EventGraphView.ResetGraph();
                        Editor.Selection.TimeActAnimationChange(entry, i);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectAnimation)
                    {
                        Editor.Selection.SelectAnimation = false;
                        Editor.EventGraphView.ResetGraph();
                        Editor.Selection.TimeActAnimationChange(entry, i);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectAnimation = true;
                    }

                    Editor.ContextMenu.TimeActAnimationMenu(isSelected, entry);

                    if (Editor.Selection.FocusAnimation)
                    {
                        Editor.Selection.FocusAnimation = false;

                        ImGui.SetScrollHereY();
                    }
                }
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }
}
