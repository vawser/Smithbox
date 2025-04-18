using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Core;

public class TimeActEventView
{
    private TimeActEditorScreen Screen;
    private ActionManager EditorActionManager;
    private TimeActSelectionManager Selection;
    private TimeActDecorator Decorator;

    public TimeActEventView(TimeActEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
        Selection = screen.Selection;
        Decorator = screen.Decorator;
    }

    public void Display()
    {
        ImGui.Begin("Events##TimeActAnimEventList");
        Selection.SwitchWindowContext(TimeActEditorContext.Event);

        if (!Selection.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventFilter", ref TimeActFilters._timeActEventFilterString, 255);
        UIHelper.ShowHoverTooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventList");
        Selection.SwitchWindowContext(TimeActEditorContext.Event);

        for (int i = 0; i < Selection.CurrentTimeActAnimation.Events.Count; i++)
        {
            TAE.Event evt = Selection.CurrentTimeActAnimation.Events[i];

            if (TimeActFilters.TimeActEventFilter(Selection.ContainerInfo, evt))
            {
                var isSelected = false;
                if (i == Selection.CurrentTimeActEventIndex ||
                    Selection.IsEventSelected(i))
                {
                    isSelected = true;
                }

                if (Selection.SelectFirstEvent)
                {
                    Selection.SelectFirstEvent = false;
                    Selection.TimeActEventChange(evt, i);
                }

                var displayName = $"{evt.TypeName}";
                if (CFG.Current.TimeActEditor_DisplayEventID)
                {
                    displayName = $"[{evt.Type}] {displayName}";
                }
                if (CFG.Current.TimeActEditor_DisplayEventBank)
                {
                    displayName = $"<{Selection.CurrentTimeAct.EventBank}> {displayName}";
                }
                displayName = $" {displayName}";

                // Event row
                if (ImGui.Selectable($@"{displayName}##taeEvent{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Selection.TimeActEventChange(evt, i);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Selection.SelectEvent)
                {
                    Selection.SelectEvent = false;
                    Selection.TimeActEventChange(evt, i);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Selection.SelectEvent = true;
                }

                if (ImGui.IsItemVisible())
                {
                    if (CFG.Current.TimeActEditor_DisplayEventRow_EnumInfo)
                        Decorator.DisplayEnumInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_ParamRefInfo)
                        Decorator.DisplayParamRefInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo)
                        Decorator.DisplayAliasEnumInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_ProjectEnumInfo)
                        Decorator.DisplayProjectEnumInfo(evt);
                }

                Selection.ContextMenu.TimeActEventMenu(isSelected, i.ToString());

                if (Selection.FocusEvent)
                {
                    Selection.FocusEvent = false;

                    ImGui.SetScrollHereY();
                }
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }
}
