using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
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
    private TimeActEditorScreen Screen;
    private TimeActSelectionManager Selection;
    private TimeActDecorator Decorator;
    private TimeActEventGraphView EventGraphView;

    public TimeActAnimationView(TimeActEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
        EventGraphView = screen.EventGraphView;
    }

    public void Display()
    {
        ImGui.Begin("Animations##TimeActAnimationList");
        Selection.SwitchWindowContext(TimeActEditorContext.Animation);

        if (!Selection.HasSelectedTimeAct())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActAnimationFilter", ref TimeActFilters._timeActAnimationFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("AnimationList");
        Selection.SwitchWindowContext(TimeActEditorContext.Animation);

        for (int i = 0; i < Selection.CurrentTimeAct.Animations.Count; i++)
        {
            TAE.Animation entry = Selection.CurrentTimeAct.Animations[i];

            if (TimeActFilters.TimeActAnimationFilter(Selection.ContainerInfo, entry))
            {
                var isSelected = false;
                if (i == Selection.CurrentTimeActAnimationIndex ||
                    Selection.IsAnimationSelected(i))
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
                    EventGraphView.ResetGraph();
                    Selection.TimeActAnimationChange(entry, i);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Selection.SelectAnimation)
                {
                    Selection.SelectAnimation = false;
                    EventGraphView.ResetGraph();
                    Selection.TimeActAnimationChange(entry, i);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Selection.SelectAnimation = true;
                }

                if (ImGui.IsItemVisible())
                {
                    if (CFG.Current.TimeActEditor_DisplayAnimRow_GeneratorInfo)
                        TimeActUtils.DisplayAnimationAlias(Selection, entry.ID);
                }

                Selection.ContextMenu.TimeActAnimationMenu(isSelected, entry.ID.ToString());

                if (Selection.FocusAnimation)
                {
                    Selection.FocusAnimation = false;

                    ImGui.SetScrollHereY();
                }
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }
}
