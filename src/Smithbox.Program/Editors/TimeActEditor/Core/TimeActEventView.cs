using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
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
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActEventView(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin("Events##TimeActAnimEventList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.Event);

        if (!Editor.Selection.HasSelectedTimeActAnimation())
        {
            ImGui.End();
            return;
        }

        ImGui.InputText($"Search##timeActEventFilter", ref Editor.Filters._timeActEventFilterString, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.BeginChild("EventList");
        Editor.Selection.SwitchWindowContext(TimeActEditorContext.Event);

        for (int i = 0; i < Editor.Selection.CurrentTimeActAnimation.Events.Count; i++)
        {
            TAE.Event evt = Editor.Selection.CurrentTimeActAnimation.Events[i];

            if (Editor.Filters.TimeActEventFilter(evt))
            {
                var isSelected = false;
                if (i == Editor.Selection.CurrentTimeActEventIndex ||
                    Editor.Selection.IsEventSelected(i))
                {
                    isSelected = true;
                }

                if (Editor.Selection.SelectFirstEvent)
                {
                    Editor.Selection.SelectFirstEvent = false;
                    Editor.Selection.TimeActEventChange(evt, i);
                }

                var displayName = $"{evt.TypeName}";
                if (CFG.Current.TimeActEditor_DisplayEventID)
                {
                    displayName = $"[{evt.Type}] {displayName}";
                }
                if (CFG.Current.TimeActEditor_DisplayEventBank)
                {
                    displayName = $"<{Editor.Selection.CurrentTimeAct.EventBank}> {displayName}";
                }
                displayName = $" {displayName}";

                // Event row
                if (ImGui.Selectable($@"{displayName}##taeEvent{i}", isSelected, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    Editor.Selection.TimeActEventChange(evt, i);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.SelectEvent)
                {
                    Editor.Selection.SelectEvent = false;
                    Editor.Selection.TimeActEventChange(evt, i);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.SelectEvent = true;
                }

                if (ImGui.IsItemVisible())
                {
                    if (CFG.Current.TimeActEditor_DisplayEventRow_EnumInfo)
                        Editor.Decorator.DisplayEnumInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_ParamRefInfo)
                        Editor.Decorator.DisplayParamRefInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo)
                        Editor.Decorator.DisplayAliasEnumInfo(evt);

                    if (CFG.Current.TimeActEditor_DisplayEventRow_ProjectEnumInfo)
                        Editor.Decorator.DisplayProjectEnumInfo(evt);
                }

                Editor.ContextMenu.TimeActEventMenu(isSelected, i.ToString());

                if (Editor.Selection.FocusEvent)
                {
                    Editor.Selection.FocusEvent = false;

                    ImGui.SetScrollHereY();
                }
            }
        }
        ImGui.EndChild();

        ImGui.End();
    }
}
