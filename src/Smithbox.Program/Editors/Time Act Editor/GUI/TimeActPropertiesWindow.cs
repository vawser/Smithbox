using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActPropertiesWindow
{
    private const int MaxTextLength = 1024;

    public TimeActEditorView View;
    public ProjectEntry Project;

    private string PropertiesFilter = "";
    private bool ExactPropertiesFilter = false;

    // The text of the fields being typed into, as it only turns into a value once the edit is done
    private Dictionary<string, string> FieldText = new();
    private TAE.Event FieldTextEvent;

    // The events an edit to the event properties goes to: every selected event when they share a type,
    // or else only the primary one
    private List<TAE.Event> EventTargets = new();

    // The same, for the properties of the time act itself
    private Dictionary<string, string> TimeActFieldText = new();
    private TimeActEntry TimeActFieldTextEntry;
    private TAE.Animation TimeActFieldTextAnimation;

    // What was selected last frame, so a new selection can bring its tab forward
    private TimeActEntry LastSelectedTimeAct;
    private TAE.Animation LastSelectedAnimation;
    private TAE.Event LastSelectedEvent;

    public TimeActPropertiesWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_PropertiesWindow_List_Header"),
            LOC.Get("TAE_PropertiesWindow_List_Header_TT"));

        DisplayHeader();

        if (!ImGui.BeginTabBar("##timeActPropertiesTabs"))
            return;

        var entry = View.Selection.SelectedTimeAct;
        var animation = View.Selection.SelectedAnimation;
        var ev = View.Selection.SelectedEvent;

        var focusTab = GetTabToFocus(entry, animation, ev);

        // Time Act
        if (ImGui.BeginTabItem($"{LOC.Get("TAE_PropertiesWindow_Tab_TimeAct")}##timeActPropertiesTab", GetTabFlags(focusTab, PropertiesTab.TimeAct)))
        {
            BeginTabContent("timeAct");

            if (entry?.TimeAct != null)
            {
                DisplayTimeActProperties(entry);
            }
            else
            {
                GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_No_TimeAct_Selected"));
            }

            EndTabContent();
        }

        // Animation
        if (ImGui.BeginTabItem($"{LOC.Get("TAE_PropertiesWindow_Tab_Animation")}##animationPropertiesTab", GetTabFlags(focusTab, PropertiesTab.Animation)))
        {
            BeginTabContent("animation");

            if (entry?.TimeAct != null && animation != null)
            {
                DisplayAnimationProperties(entry, animation);
            }
            else
            {
                GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_No_Animation_Selected"));
            }

            EndTabContent();
        }

        // Event
        if (ImGui.BeginTabItem($"{LOC.Get("TAE_PropertiesWindow_Tab_Event")}##eventPropertiesTab", GetTabFlags(focusTab, PropertiesTab.Event)))
        {
            BeginTabContent("event");

            if (ev != null)
            {
                DisplayEventProperties(ev);
            }
            else
            {
                GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_No_Event_Selected"));
            }

            EndTabContent();
        }

        ImGui.EndTabBar();
    }

    /// <summary>
    /// The filter, with buttons after it for how the parameters are named, and whether unused ones are shown
    /// </summary>
    private void DisplayHeader()
    {
        const string id = "timeActEditor_Properties_Filter";

        ImGui.BeginChild($"framedListFilter_{id}", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar(id, ref PropertiesFilter, ref ExactPropertiesFilter);

        // Field Name Mode
        ImGui.SameLine();

        var mode = CFG.Current.TimeActEditor_Field_Name_Mode;
        var next = mode is TimeActFieldNameMode.Annotation ? TimeActFieldNameMode.Internal : TimeActFieldNameMode.Annotation;

        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{(mode is TimeActFieldNameMode.Annotation ? Icons.Book : Icons.Code)}##timeActFieldNameMode"))
        {
            CFG.Current.TimeActEditor_Field_Name_Mode = next;
        }
        GUI.Tooltip(LOC.Get("TAE_PropertiesWindow_Field_Name_Mode_TT",
            GetFieldNameModeName(mode), GetFieldNameModeName(next)));

        // Obsolete Fields
        ImGui.SameLine();

        var hidden = CFG.Current.TimeActEditor_Hide_Obsolete_Fields;

        if (ImGui.Button($"{(hidden ? Icons.EyeSlash : Icons.Eye)}##timeActHideObsoleteFields"))
        {
            CFG.Current.TimeActEditor_Hide_Obsolete_Fields = !hidden;
        }
        GUI.Tooltip(LOC.Get(hidden
            ? "TAE_PropertiesWindow_Obsolete_Fields_Hidden_TT"
            : "TAE_PropertiesWindow_Obsolete_Fields_Shown_TT"));

        ImGui.EndChild();
    }

    private static string GetFieldNameModeName(TimeActFieldNameMode mode) => mode switch
    {
        TimeActFieldNameMode.Internal => LOC.Get("TAE_PropertiesWindow_Field_Name_Mode_Internal"),
        _ => LOC.Get("TAE_PropertiesWindow_Field_Name_Mode_Annotation")
    };

    private enum PropertiesTab
    {
        None,
        TimeAct,
        Animation,
        Event
    }

    /// <summary>
    /// Gets the tab of whatever was just selected, or None if the selection hasn't changed.
    /// Selecting a time act also selects its first animation, and selecting an animation clears the event,
    /// so the outermost change is the one that was picked.
    /// </summary>
    private PropertiesTab GetTabToFocus(TimeActEntry entry, TAE.Animation animation, TAE.Event ev)
    {
        var tab = PropertiesTab.None;

        if (entry != null && entry != LastSelectedTimeAct)
        {
            tab = PropertiesTab.TimeAct;
        }
        else if (animation != null && animation != LastSelectedAnimation)
        {
            tab = PropertiesTab.Animation;
        }
        else if (ev != null && ev != LastSelectedEvent)
        {
            tab = PropertiesTab.Event;
        }

        LastSelectedTimeAct = entry;
        LastSelectedAnimation = animation;
        LastSelectedEvent = ev;

        return tab;
    }

    private static ImGuiTabItemFlags GetTabFlags(PropertiesTab focusTab, PropertiesTab tab)
    {
        return focusTab == tab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;
    }

    private static void BeginTabContent(string id)
    {
        ImGui.BeginChild($"PropertiesSection_{id}", new Vector2(0, 0), ImGuiChildFlags.Borders);
    }

    private static void EndTabContent()
    {
        ImGui.EndChild();
        ImGui.EndTabItem();
    }

    private void DisplayEventProperties(TAE.Event ev)
    {
        if (FieldTextEvent != ev)
        {
            FieldTextEvent = ev;
            FieldText.Clear();
        }

        var selected = View.Selection.SelectedEvents;
        var sameType = View.Selection.AreSelectedEventsSameType();

        EventTargets.Clear();

        if (sameType)
        {
            EventTargets.AddRange(selected);
        }
        else
        {
            EventTargets.Add(ev);
        }

        if (selected.Count > 1)
        {
            GUI.WrappedText(sameType
                ? LOC.Get("TAE_PropertiesWindow_Multiple_Events_Same_Type", selected.Count)
                : LOC.Get("TAE_PropertiesWindow_Multiple_Events_Different_Type", selected.Count));

            GUI.Spacer();
        }

        ImGui.BeginDisabled(!Project.Handler.TimeActData.CanSaveTimeActs);

        if (ImGui.BeginTable("##eventProperties", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
        {
            // Each event keeps its own other end, so setting the start of several doesn't line up their ends
            DisplayTimeField("TAE_PropertiesWindow_Start_Time", ev.StartTime, (target, value) => (value, target.EndTime));
            DisplayTimeField("TAE_PropertiesWindow_End_Time", ev.EndTime, (target, value) => (target.StartTime, value));

            if (ev.Parameters != null)
            {
                foreach (var (key, parameter) in ev.Parameters.Template)
                {
                    // Fixed values aren't parameters as such, and the writer fills them in
                    if (parameter.ValueToAssert != null)
                        continue;

                    var meta = Project.Handler.TimeActData.GetFieldMeta(ev, parameter.Name);

                    if (meta?.Obsolete == true && CFG.Current.TimeActEditor_Hide_Obsolete_Fields)
                        continue;

                    var annotation = Project.Handler.TimeActData.GetFieldAnnotation(ev, parameter.Name);

                    // Both names match, whichever is on show
                    if (!EditorFilters.IsMatch(PropertiesFilter, parameter.Name, ExactPropertiesFilter, annotation?.Name ?? ""))
                        continue;

                    ImGui.PushID(key);

                    DisplayParameter(ev, key, parameter, meta);
                    DisplayParameterMeta(ev, key, meta);

                    ImGui.PopID();

                    if (meta?.Separator == true)
                    {
                        DisplaySeparatorRow();
                    }
                }
            }

            ImGui.EndTable();
        }

        ImGui.EndDisabled();

        if (View.Playback.EventDisplay.GetStatus(ev) is { } status)
        {
            GUI.Spacer();
            GUI.WrappedText(status);
        }

        if (ev.Parameters == null)
        {
            GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_No_Template", ev.Type));

            var bytes = ev.GetParameterBytes(View.Selection.SelectedTimeAct.TimeAct.BigEndian);

            GUI.WrappedText(Convert.ToHexString(bytes));
        }
    }

    private delegate bool TryParseValue<T>(string text, out T value);

    /// <summary>
    /// The properties of the time act's header. The format can't be changed, as it decides how the rest is laid out.
    /// </summary>
    private void DisplayTimeActProperties(TimeActEntry entry)
    {
        var tae = entry.TimeAct;

        if (TimeActFieldTextEntry != entry)
        {
            TimeActFieldTextEntry = entry;
            TimeActFieldText.Clear();
        }

        GUI.SimpleHeader(
            LOC.Get("TAE_PropertiesWindow_TimeAct_Header", entry.Name),
            LOC.Get("TAE_PropertiesWindow_TimeAct_Header_TT"));

        if (!ImGui.BeginTable("##timeActProperties", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
            return;

        // Format
        var format = tae.BigEndian ? LOC.Get("TAE_PropertiesWindow_TimeAct_Format_Big_Endian", tae.Format) : $"{tae.Format}";

        DisplayReadOnlyField("TAE_PropertiesWindow_TimeAct_Format", format);

        // Animation Count
        DisplayReadOnlyField("TAE_PropertiesWindow_TimeAct_Animation_Count", $"{tae.Animations.Count}");

        ImGui.BeginDisabled(!Project.Handler.TimeActData.CanSaveTimeActs);

        // ID
        DisplayTimeActField(entry, "TAE_PropertiesWindow_TimeAct_ID", tae.ID, value => $"{value}",
            int.TryParse, value => tae.ID = value);

        // Flags
        DisplayTimeActField(entry, "TAE_PropertiesWindow_TimeAct_Flags", tae.Flags, FormatFlags,
            TryParseFlags, value => tae.Flags = value);

        // Event Bank: not stored in the older formats
        if (tae.Format is not (TAE.TAEFormat.DS1 or TAE.TAEFormat.DES))
        {
            DisplayTimeActField(entry, "TAE_PropertiesWindow_TimeAct_Event_Bank", tae.EventBank, value => $"{value}",
                long.TryParse, value => tae.EventBank = value);
        }

        // Skeleton and SIB names: not stored in the Dark Souls II format
        if (tae.Format is not TAE.TAEFormat.SOTFS)
        {
            DisplayTimeActField(entry, "TAE_PropertiesWindow_TimeAct_Skeleton_Name", tae.SkeletonName, value => value ?? "",
                TryParseName, value => tae.SkeletonName = value);

            DisplayTimeActField(entry, "TAE_PropertiesWindow_TimeAct_Sib_Name", tae.SibName, value => value ?? "",
                TryParseName, value => tae.SibName = value);
        }

        ImGui.EndDisabled();

        ImGui.EndTable();
    }

    private void DisplayReadOnlyField(string nameKey, string value)
    {
        var name = LOC.Get(nameKey);

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter))
            return;

        BeginRow(name);

        ImGui.TextUnformatted(value);
        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));
    }

    /// <summary>
    /// A text field for a time act or animation property. The value is only changed once the text is done being
    /// edited and is valid. Values are compared by their text, so arrays are compared by their contents.
    /// The validation returns the reason a value can't be used, or null if it can.
    /// </summary>
    private void DisplayTimeActField<T>(TimeActEntry entry, string nameKey, T value, Func<T, string> format,
        TryParseValue<T> parse, Action<T> set, Func<T, string> validate = null, Action onChanged = null)
    {
        var name = LOC.Get(nameKey);

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter))
            return;

        BeginRow(name);

        var valueText = format(value);
        var text = TimeActFieldText.TryGetValue(nameKey, out var typed) ? typed : valueText;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{nameKey}", ref text, MaxTextLength))
        {
            TimeActFieldText[nameKey] = text;
        }

        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            TimeActFieldText.Remove(nameKey);

            if (!parse(text, out var newValue))
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_TimeAct_Invalid_Value", text, name));
                return;
            }

            if (format(newValue) == valueText)
                return;

            if (validate?.Invoke(newValue) is { } error)
            {
                Smithbox.Log(this, error);
                return;
            }

            View.ViewportActionManager.ExecuteAction(new TimeActSetPropertyAction<T>(entry, set, value, newValue, onChanged));
        }
    }

    /// <summary>
    /// A checkbox for a time act or animation property, which changes the value as soon as it is ticked
    /// </summary>
    private void DisplayTimeActCheckbox(TimeActEntry entry, string nameKey, bool value, Action<bool> set,
        Action onChanged = null)
    {
        var name = LOC.Get(nameKey);

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter))
            return;

        BeginRow(name);

        var ticked = value;

        if (ImGui.Checkbox($"##{nameKey}", ref ticked))
        {
            View.ViewportActionManager.ExecuteAction(new TimeActSetPropertyAction<bool>(entry, set, value, ticked, onChanged));
        }

        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));
    }

    /// <summary>
    /// The properties of the selected animation: its ID, file name and the mini header, which decides
    /// whether the animation plays its own motion or imports it from another.
    /// </summary>
    private void DisplayAnimationProperties(TimeActEntry entry, TAE.Animation animation)
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;
        var tae = entry.TimeAct;

        // The text being typed belongs to the animation it was typed for
        if (TimeActFieldTextAnimation != animation)
        {
            TimeActFieldTextAnimation = animation;
            TimeActFieldText.Clear();
        }

        GUI.SimpleHeader(
            LOC.Get("TAE_PropertiesWindow_Animation_Header", source.GetAnimationName(entry, animation)),
            LOC.Get("TAE_PropertiesWindow_Animation_Header_TT"));

        if (!ImGui.BeginTable("##animationProperties", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
            return;

        // Event Count
        DisplayReadOnlyField("TAE_PropertiesWindow_Animation_Event_Count", $"{animation.Events.Count}");

        ImGui.BeginDisabled(!Project.Handler.TimeActData.CanSaveTimeActs);

        // ID: the animations are kept in ID order, and the name and motion come from it
        DisplayTimeActField(entry, "TAE_PropertiesWindow_Animation_ID", animation.ID, value => $"{value}",
            long.TryParse, value => animation.ID = value,
            validate: value => tae.Animations.Any(e => e != animation && e.ID == value)
                ? LOC.Get("TAE_PropertiesWindow_Animation_ID_In_Use", value) : null,
            onChanged: () =>
            {
                entry.ResortAnimation(animation);
                OnAnimationMotionChanged(animation);
            });

        // File Name
        DisplayTimeActField(entry, "TAE_PropertiesWindow_Animation_File_Name", animation.AnimFileName, value => value ?? "",
            TryParseText, value => animation.AnimFileName = value);

        // Mini Header Type
        DisplayMiniHeaderType(entry, animation);

        switch (animation.MiniHeader)
        {
            case TAE.Animation.AnimMiniHeader.Standard standard:
                DisplayTimeActCheckbox(entry, "TAE_PropertiesWindow_Animation_Loop_By_Default", standard.IsLoopByDefault,
                    value => standard.IsLoopByDefault = value);

                DisplayTimeActCheckbox(entry, "TAE_PropertiesWindow_Animation_Imports_HKX", standard.ImportsHKX,
                    value => standard.ImportsHKX = value, () => OnAnimationMotionChanged(animation));

                DisplayTimeActField(entry, "TAE_PropertiesWindow_Animation_Import_HKX_Source", standard.ImportHKXSourceAnimID,
                    value => $"{value}", int.TryParse, value => standard.ImportHKXSourceAnimID = value,
                    onChanged: () => OnAnimationMotionChanged(animation));

                // Not stored in the Demon's Souls formats
                if (tae.Format is not (TAE.TAEFormat.DES or TAE.TAEFormat.DESR))
                {
                    DisplayTimeActCheckbox(entry, "TAE_PropertiesWindow_Animation_Allow_Delay_Load", standard.AllowDelayLoad,
                        value => standard.AllowDelayLoad = value);
                }
                break;

            case TAE.Animation.AnimMiniHeader.ImportOtherAnim import:
                DisplayTimeActField(entry, "TAE_PropertiesWindow_Animation_Import_From", import.ImportFromAnimID,
                    value => $"{value}", int.TryParse, value => import.ImportFromAnimID = value,
                    onChanged: () => OnAnimationMotionChanged(animation));

                DisplayTimeActField(entry, "TAE_PropertiesWindow_Animation_Import_Unknown", import.Unknown,
                    value => $"{value}", int.TryParse, value => import.Unknown = value);
                break;
        }

        ImGui.EndDisabled();

        ImGui.EndTable();
    }

    /// <summary>
    /// Switching the type replaces the mini header with a new one, so undoing it brings back the old values
    /// </summary>
    private void DisplayMiniHeaderType(TimeActEntry entry, TAE.Animation animation)
    {
        var nameKey = "TAE_PropertiesWindow_Animation_Mini_Header_Type";
        var name = LOC.Get(nameKey);

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter))
            return;

        BeginRow(name);

        var current = animation.MiniHeader;

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##{nameKey}", GetMiniHeaderTypeName(current?.Type)))
        {
            foreach (var type in Enum.GetValues<TAE.Animation.MiniHeaderType>())
            {
                if (ImGui.Selectable(GetMiniHeaderTypeName(type), current?.Type == type) && current?.Type != type)
                {
                    TAE.Animation.AnimMiniHeader header = type is TAE.Animation.MiniHeaderType.Standard
                        ? new TAE.Animation.AnimMiniHeader.Standard()
                        : new TAE.Animation.AnimMiniHeader.ImportOtherAnim();

                    View.ViewportActionManager.ExecuteAction(new TimeActSetPropertyAction<TAE.Animation.AnimMiniHeader>(
                        entry, value => animation.MiniHeader = value, current, header,
                        () => OnAnimationMotionChanged(animation)));
                }
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));
    }

    private static string GetMiniHeaderTypeName(TAE.Animation.MiniHeaderType? type)
    {
        return type switch
        {
            TAE.Animation.MiniHeaderType.Standard => LOC.Get("TAE_PropertiesWindow_Animation_Mini_Header_Standard"),
            TAE.Animation.MiniHeaderType.ImportOtherAnim => LOC.Get("TAE_PropertiesWindow_Animation_Mini_Header_Import"),
            _ => ""
        };
    }

    /// <summary>
    /// The motion that plays is resolved from the ID and mini header, so it is looked up again
    /// </summary>
    private void OnAnimationMotionChanged(TAE.Animation animation)
    {
        if (View.Selection.SelectedAnimation == animation)
        {
            View.Playback.OnAnimationChanged();
        }
    }

    private static bool TryParseText(string text, out string value)
    {
        value = text ?? "";
        return true;
    }

    private static string FormatFlags(byte[] flags)
    {
        return flags == null ? "" : string.Join(" ", flags.Select(e => $"{e:X2}"));
    }

    /// <summary>
    /// The flags are always written as 8 bytes, typed as hex with or without spaces
    /// </summary>
    private static bool TryParseFlags(string text, out byte[] flags)
    {
        flags = null;

        var hex = text.Replace(" ", "");

        if (hex.Length != 16)
            return false;

        try
        {
            flags = Convert.FromHexString(hex);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// An empty name is left out of the file, as it is when the game has none
    /// </summary>
    private static bool TryParseName(string text, out string name)
    {
        name = string.IsNullOrEmpty(text) ? null : text;
        return true;
    }

    private void DisplayTimeField(string nameKey, float value, Func<TAE.Event, float, (float Start, float End)> apply)
    {
        var text = BeginField(LOC.Get(nameKey), nameKey, value.ToString());

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{nameKey}", ref text, MaxTextLength))
        {
            FieldText[nameKey] = text;
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(nameKey);

            if (float.TryParse(text, out var newValue))
            {
                var actions = new List<ViewportAction>();

                foreach (var target in EventTargets)
                {
                    var (start, end) = apply(target, newValue);

                    if (start == target.StartTime && end == target.EndTime)
                        continue;

                    actions.Add(new TimeActMoveEventAction(View.Selection.SelectedTimeAct, target,
                        target.StartTime, target.EndTime, start, end));
                }

                ExecuteActions(actions);
            }
        }
    }

    private void DisplayParameter(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter, TimeActField meta)
    {
        var value = ev.Parameters[key];
        var name = Project.Handler.TimeActData.GetFieldDisplayName(ev, parameter.Name);

        if (parameter.EnumEntries != null)
        {
            DisplayEnumParameter(ev, key, parameter, meta, name, value);
            return;
        }

        if (parameter.Type is TAE.Template.ParamType.b)
        {
            BeginParameterRow(ev, key, parameter, meta, name);

            var ticked = (bool)value;

            if (ImGui.Checkbox($"##{key}", ref ticked))
            {
                SetParameter(ev, key, ticked);
            }

            DisplayParameterTooltip(ev, parameter, meta);
            DisplayParameterContextMenu(ev, key, parameter, meta, value);
            return;
        }

        // A number that is only ever 0 or 1
        if (meta?.IsBool == true && IsInteger(value))
        {
            BeginParameterRow(ev, key, parameter, meta, name);

            var ticked = Convert.ToInt64(value) != 0;

            if (ImGui.Checkbox($"##{key}", ref ticked))
            {
                SetParameter(ev, key, Convert.ChangeType(ticked ? 1 : 0, value.GetType()));
            }

            DisplayParameterTooltip(ev, parameter, meta);
            DisplayParameterContextMenu(ev, key, parameter, meta, value);
            return;
        }

        BeginParameterRow(ev, key, parameter, meta, name);

        var text = FieldText.TryGetValue(key, out var typed) ? typed : parameter.ValueToString(value);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{key}", ref text, MaxTextLength))
        {
            FieldText[key] = text;
        }

        DisplayParameterTooltip(ev, parameter, meta);

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(key);

            try
            {
                SetParameter(ev, key, parameter.StringToValue(text));
            }
            catch (Exception e) when (e is FormatException or OverflowException or IndexOutOfRangeException)
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_Invalid_Value", text, parameter.Type));
            }
        }

        DisplayParameterContextMenu(ev, key, parameter, meta, value);
    }

    private static bool IsInteger(object value)
    {
        return value is byte or sbyte or short or ushort or int or uint or long or ulong;
    }

    /// <summary>
    /// The tooltip of a parameter gives the name that isn't on show, its type, what it does, and its default
    /// </summary>
    private void DisplayParameterTooltip(TAE.Event ev, TAE.Template.ParameterTemplate parameter, TimeActField meta)
    {
        var lines = new List<string>();
        var annotation = Project.Handler.TimeActData.GetFieldAnnotation(ev, parameter.Name);

        if (annotation != null && annotation.Name.Length > 0)
        {
            lines.Add(CFG.Current.TimeActEditor_Field_Name_Mode is TimeActFieldNameMode.Annotation
                ? LOC.Get("TAE_PropertiesWindow_Field_Internal_Name", parameter.Name)
                : LOC.Get("TAE_PropertiesWindow_Field_Annotation_Name", annotation.Name));
        }

        lines.Add(parameter.Type.ToString());

        if (meta != null && meta.Obsolete)
        {
            lines.Add(LOC.Get("TAE_PropertiesWindow_Field_Obsolete"));
        }

        if (annotation != null && annotation.Description.Length > 0)
        {
            lines.Add(annotation.Description);
        }

        if (meta != null && meta.DefaultValue.Length > 0)
        {
            lines.Add(LOC.Get("TAE_PropertiesWindow_Field_Default", meta.DefaultValue));
        }

        GUI.Tooltip(string.Join("\n", lines));
    }

    /// <summary>
    /// The right-click menu of a parameter, which can put it back to its default
    /// </summary>
    private void DisplayParameterContextMenu(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter,
        TimeActField meta, object value)
    {
        if (meta == null || meta.DefaultValue.Length == 0)
            return;

        if (!ImGui.BeginPopupContextItem($"##{key}_contextMenu"))
            return;

        object defaultValue = null;

        try
        {
            defaultValue = parameter.StringToValue(meta.DefaultValue);
        }
        catch (Exception e) when (e is FormatException or OverflowException or IndexOutOfRangeException)
        {
        }

        ImGui.BeginDisabled(defaultValue == null || defaultValue.Equals(value));

        if (ImGui.MenuItem(LOC.Get("TAE_PropertiesWindow_Field_Reset_To_Default", meta.DefaultValue)))
        {
            FieldText.Remove(key);
            SetParameter(ev, key, defaultValue);
        }

        ImGui.EndDisabled();

        if (defaultValue == null)
        {
            GUI.Tooltip(LOC.Get("TAE_PropertiesWindow_Field_Invalid_Default", meta.DefaultValue, parameter.Type));
        }

        ImGui.EndPopup();
    }

    /// <summary>
    /// Adds rows under a parameter that refers to something, showing what it refers to
    /// </summary>
    private void DisplayParameterMeta(TAE.Event ev, string key, TimeActField meta)
    {
        if (meta == null)
            return;

        DisplayParamReference(ev, key, meta);
        DisplayBehaviorReference(ev, key, meta);
        DisplayProjectEnum(ev, key, meta);
        DisplayAlias(ev, key, meta);
    }

    private void DisplayParamReference(TAE.Event ev, string key, TimeActField meta)
    {
        if (meta.ParamRef == "" || !CFG.Current.ParamEditor_Field_List_Display_References ||
            Project.Handler.ParamEditor?.ViewHandler.ActiveView is not { } paramView)
        {
            return;
        }

        var bank = paramView.GetPrimaryBank().Params;

        // Not every game has every param
        var refs = meta.ParamRef.Split(',')
            .Where(name => bank != null && bank.ContainsKey(name))
            .Select(name => new ParamRef(null, name))
            .ToList();

        if (refs.Count == 0)
            return;

        object value = ev.Parameters[key];
        object newValue = null;

        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ParamReferenceHelper.Label(paramView, refs, null);

        ImGui.TableSetColumnIndex(1);
        ParamReferenceHelper.Hint(paramView, refs, null, value);
        ParamReferenceHelper.Click(paramView, value, null, refs);

        if (ImGui.BeginPopupContextItem("##paramRefContextMenu"))
        {
            var changed = ParamReferenceHelper.ContextMenu(paramView, refs, null, value, ref newValue, null);

            ImGui.EndPopup();

            if (changed && newValue != null)
            {
                SetParameter(ev, key, newValue);
            }
        }
    }

    // More rows than this are left to the context menu
    private const int BehaviorHintRows = 4;
    private const int BehaviorMenuRows = 32;

    /// <summary>
    /// The BehaviorParam rows a behavior judge ID picks, as quick links: ctrl-click goes to the best one,
    /// and the context menu goes to any of them. The judge ID isn't the row ID, so the rows come from the
    /// behavior variation of the character rather than from a param reference.
    /// </summary>
    private void DisplayBehaviorReference(TAE.Event ev, string key, TimeActField meta)
    {
        if (meta.BehaviorRef == "" || !CFG.Current.ParamEditor_Field_List_Display_References ||
            Project.Handler.ParamEditor?.ViewHandler.ActiveView is not { } paramView ||
            View.Selection.SelectedTimeActContainerWrapper is not { } source)
        {
            return;
        }

        object value = ev.Parameters[key];

        if (!IsInteger(value))
            return;

        var paramName = TimeActEventResolver.GetBehaviorParamName(source.Name);

        if (paramView.GetPrimaryBank().Params is not { } bank || !bank.ContainsKey(paramName))
            return;

        var common = meta.BehaviorRef.Equals("Common", StringComparison.OrdinalIgnoreCase);
        var rows = View.Playback.EventDisplay.GetBehaviorRows(ev, source.Name, Convert.ToInt64(value), common);

        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ParamReferenceHelper.Label(paramView, new List<ParamRef> { new(null, paramName) }, null);

        ImGui.TableSetColumnIndex(1);
        ImGui.BeginGroup();

        if (rows.Count == 0)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("---");
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);

            foreach (var row in rows.Take(BehaviorHintRows))
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextUnformatted(GetBehaviorRowName(row));
            }

            ImGui.PopStyleColor();

            if (rows.Count > BehaviorHintRows)
            {
                ImGui.TextDisabled(LOC.Get("TAE_PropertiesWindow_Behavior_More_Rows", rows.Count - BehaviorHintRows));
            }
        }

        ImGui.EndGroup();

        if (rows.Count > 0)
        {
            GUI.Tooltip(LOC.Get("TAE_PropertiesWindow_Behavior_Link_TT"));

            if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
            {
                var view = InputManager.HasShiftDown() ? "new" : "-1";
                EditorCommandQueue.AddCommand($@"param/select/{view}/{paramName}/{rows[0].ID}");
            }
        }

        if (rows.Count > 0 && ImGui.BeginPopupContextItem("##behaviorRefContextMenu"))
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

            var index = 0;

            foreach (var row in rows.Take(BehaviorMenuRows))
            {
                var name = GetBehaviorRowName(row);

                if (ImGui.Selectable($"{LOC.Get("PARAM_FieldDecorator_ParamRef_Go_To_Action", name)}##behaviorGoTo{index}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{paramName}/{row.ID}");
                }

                if (ImGui.Selectable($"{LOC.Get("PARAM_FieldDecorator_ParamRef_Go_To_New_View_Action", name)}##behaviorGoToNew{index}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/new/{paramName}/{row.ID}");
                }

                index++;
            }

            ImGui.PopStyleColor();

            ImGui.EndPopup();
        }
    }

    private static string GetBehaviorRowName(Andre.Formats.Param.Row row)
    {
        return string.IsNullOrWhiteSpace(row.Name)
            ? LOC.Get("TAE_PropertiesWindow_Behavior_Unnamed_Row", row.ID)
            : $"{row.ID}: {row.Name}";
    }

    /// <summary>
    /// The name of the value in the project enum, which can be right-clicked to pick another
    /// </summary>
    private void DisplayProjectEnum(TAE.Event ev, string key, TimeActField meta)
    {
        if (meta.ProjectEnum == "" || !CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var projectEnum = ProjectEnumHelper.GetEnum(Project, meta.ProjectEnum);

        if (projectEnum == null)
            return;

        object value = ev.Parameters[key];
        object newValue = null;

        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ProjectEnumHelper.Label(Project, meta.ProjectEnum);

        ImGui.TableSetColumnIndex(1);
        ProjectEnumHelper.Hint(Project, meta.ProjectEnum, $"{value}");

        if (ImGui.BeginPopupContextItem("##projectEnumContextMenu"))
        {
            var changed = ProjectEnumHelper.ContextMenu(projectEnum, value, ref newValue);

            ImGui.EndPopup();

            if (changed && newValue != null)
            {
                SetParameter(ev, key, newValue);
            }
        }
    }

    /// <summary>
    /// The alias of the value, under the field, which can be right-clicked to pick another from the aliases
    /// </summary>
    private void DisplayAlias(TAE.Event ev, string key, TimeActField meta)
    {
        if (meta.Alias == "" || !CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        if (!Enum.TryParse(meta.Alias, out ProjectAliasType aliasType) || aliasType is ProjectAliasType.None)
            return;

        if (!Project.Handler.ProjectData.Aliases.TryGetValue(aliasType, out var aliases) || aliases == null)
            return;

        object value = ev.Parameters[key];

        if (!IsInteger(value))
            return;

        var source = View.Selection.SelectedTimeActContainerWrapper;
        var alias = GetAliasName(aliasType, aliases, source, Convert.ToInt64(value));

        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
        ImGui.TextUnformatted($"   {LOC.Get(aliasType.GetDisplayName())}");
        ImGui.PopStyleColor();

        ImGui.TableSetColumnIndex(1);

        if (alias != "")
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
            ImGui.TextUnformatted(alias);
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.TextDisabled(LOC.Get("TAE_PropertiesWindow_Alias_Not_Found"));
        }

        if (ImGui.BeginPopupContextItem("##aliasContextMenu"))
        {
            object newValue = null;
            var choices = GetAliasChoices(aliasType, aliases, source);
            var changed = AliasEnumHelper.ContextMenu(null, choices, value, ref newValue);

            ImGui.EndPopup();

            if (changed && newValue != null)
            {
                SetParameter(ev, key, newValue);
            }
        }
    }

    /// <summary>
    /// Animation aliases belong to a character and are named by the HKX file (c0000_a000_003000), so the value is
    /// looked up as an animation of the time act's character. Character aliases are named c1000 for the value 1000.
    /// Every other alias is named by the value itself.
    /// </summary>
    private string GetAliasName(ProjectAliasType aliasType, List<AliasEntry> aliases, TimeActContainerWrapper source, long id)
    {
        if (id < 0)
            return "";

        return aliasType switch
        {
            ProjectAliasType.Animations => source == null ? "" :
                AliasHelper.GetAnimationAlias(Project, source.Name, source.FormatAnimationName(id)),
            ProjectAliasType.Characters =>
                aliases.FirstOrDefault(e => TryParseAliasNumber(e.ID, out var n) && n == id)?.Name ?? "",
            _ => AliasHelper.GetAliasName(aliases, $"{id}")
        };
    }

    // The aliases offered by the right-click menu, with the value each stands for as its ID
    private (ProjectAliasType Type, List<AliasEntry> Source, int Count, string Character) AliasChoicesKey;
    private List<AliasEntry> AliasChoices = new();

    private static readonly Regex AnimationAliasRegex = new(@"(?:^|_)a(\d+)_(\d+)$", RegexOptions.Compiled);

    private List<AliasEntry> GetAliasChoices(ProjectAliasType aliasType, List<AliasEntry> aliases, TimeActContainerWrapper source)
    {
        var cacheKey = (aliasType, aliases, aliases.Count, source?.Name);

        if (AliasChoicesKey == cacheKey)
            return AliasChoices;

        var choices = new List<AliasEntry>();

        if (aliasType is ProjectAliasType.Animations)
        {
            // The character's own animations, then those of the sets it plays, then those named for every character
            var prefixes = source == null ? new List<string>() :
                StudioCore.Renderer.AnimationPackage.GetAnimationSetNames(StudioCore.Renderer.AnimatedModelKind.Character, source.Name)
                    .Select(set => $"{set}_")
                    .ToList();

            var seen = new HashSet<long>();

            foreach (var prefix in prefixes.Append(""))
            {
                foreach (var entry in aliases)
                {
                    if (entry?.ID == null)
                        continue;

                    var isMatch = prefix == ""
                        ? entry.ID.StartsWith('a')
                        : entry.ID.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

                    if (!isMatch)
                        continue;

                    var match = AnimationAliasRegex.Match(entry.ID);

                    if (!match.Success)
                        continue;

                    var size = (long)Math.Pow(10, match.Groups[2].Value.Length);
                    var id = long.Parse(match.Groups[1].Value) * size + long.Parse(match.Groups[2].Value);

                    if (seen.Add(id))
                    {
                        choices.Add(new AliasEntry { ID = $"{id}", Name = entry.Name, Description = entry.Description, Tags = entry.Tags });
                    }
                }
            }
        }
        else
        {
            // Characters come from the project's files, so one without an alias can still be picked
            var entries = aliasType is ProjectAliasType.Characters
                ? ModelFileChoices.CharacterAliasEntries(Project)
                : aliases;

            foreach (var entry in entries)
            {
                if (entry?.ID != null && TryParseAliasNumber(entry.ID, out var id))
                {
                    choices.Add(new AliasEntry { ID = $"{id}", Name = entry.Name, Description = entry.Description, Tags = entry.Tags });
                }
            }
        }

        AliasChoicesKey = cacheKey;
        AliasChoices = choices;

        return choices;
    }

    /// <summary>
    /// The number an alias stands for: its ID, or its ID after a letter prefix such as the c of c1000
    /// </summary>
    private static bool TryParseAliasNumber(string aliasId, out long number)
    {
        var digits = aliasId.TrimStart('c', 'C');

        return long.TryParse(digits, out number);
    }

    private static void DisplaySeparatorRow()
    {
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.Separator();

        ImGui.TableSetColumnIndex(1);
        ImGui.Separator();
    }

    private void DisplayEnumParameter(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter,
        TimeActField meta, string name, object value)
    {
        BeginParameterRow(ev, key, parameter, meta, name);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo($"##{key}", parameter.ValueToString(value)))
        {
            foreach (var (entryName, entryValue) in parameter.EnumEntries)
            {
                if (ImGui.Selectable(entryName, entryValue.Equals(value)))
                {
                    SetParameter(ev, key, entryValue);
                }
            }

            ImGui.EndCombo();
        }

        DisplayParameterTooltip(ev, parameter, meta);
        DisplayParameterContextMenu(ev, key, parameter, meta, value);
    }

    /// <summary>
    /// Sets the parameter on every event being edited. The value comes from the field of the primary event,
    /// which is passed along so that it is clear where it was read from.
    /// </summary>
    private void SetParameter(TAE.Event ev, string key, object value)
    {
        var actions = new List<ViewportAction>();

        foreach (var target in EventTargets.Count > 0 ? EventTargets : [ev])
        {
            if (target.Parameters == null || !target.Parameters.Values.ContainsKey(key))
                continue;

            if (value.Equals(target.Parameters[key]))
                continue;

            actions.Add(new TimeActSetParameterAction(View.Selection.SelectedTimeAct, target, key, value));
        }

        ExecuteActions(actions);
    }

    /// <summary>
    /// Edits to several events go in as one action, so that a single undo takes them all back
    /// </summary>
    private void ExecuteActions(List<ViewportAction> actions)
    {
        if (actions.Count == 1)
        {
            View.ViewportActionManager.ExecuteAction(actions[0]);
        }
        else if (actions.Count > 1)
        {
            View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actions));
        }
    }

    /// <summary>
    /// Starts a row with its name. Obsolete fields have their name dimmed, as the game doesn't read them.
    /// </summary>
    private static void BeginRow(string name, bool isDimmed = false)
    {
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();

        if (isDimmed)
        {
            ImGui.TextDisabled(name);
        }
        else
        {
            ImGui.Text(name);
        }

        ImGui.TableSetColumnIndex(1);
    }

    /// <summary>
    /// The name cell of a parameter. Hovering it gives the annotation: the other name and the description.
    /// Right-clicking it lists both names and the description, and copies either name.
    /// </summary>
    private void BeginParameterRow(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter,
        TimeActField meta, string name)
    {
        ImGui.TableNextRow();

        ImGui.TableSetColumnIndex(0);
        ImGui.AlignTextToFramePadding();

        if (meta?.Obsolete == true)
        {
            ImGui.TextDisabled(name);
        }
        else
        {
            ImGui.Text(name);
        }

        DisplayParameterTooltip(ev, parameter, meta);
        DisplayParameterNameContextMenu(ev, key, parameter);

        ImGui.TableSetColumnIndex(1);
    }

    private void DisplayParameterNameContextMenu(TAE.Event ev, string key, TAE.Template.ParameterTemplate parameter)
    {
        if (!ImGui.BeginPopupContextItem($"##{key}_nameContextMenu"))
            return;

        var annotation = Project.Handler.TimeActData.GetFieldAnnotation(ev, parameter.Name);
        var annotationName = annotation?.Name ?? "";
        var description = annotation?.Description ?? "";

        if (ImGui.BeginMenu($"{LOC.Get("TAE_PropertiesWindow_Field_Info_Header")}##{key}_infoMenu"))
        {
            ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f),
                LOC.Get("TAE_PropertiesWindow_Field_Internal_Name", parameter.Name));

            if (annotationName.Length > 0)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f),
                    LOC.Get("TAE_PropertiesWindow_Field_Annotation_Name", annotationName));
            }
            else
            {
                ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.7f),
                    LOC.Get("TAE_PropertiesWindow_Field_No_Annotation_Name"));
            }

            if (description.Length > 0)
            {
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 30);
                ImGui.TextColored(new Vector4(.4f, .7f, 1f, 1f), description);
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.7f),
                    LOC.Get("TAE_PropertiesWindow_Field_No_Description"));
            }

            ImGui.EndMenu();
        }

        if (ImGui.MenuItem($"{LOC.Get("TAE_PropertiesWindow_Field_Copy_Internal_Name")}##{key}_copyInternalName"))
        {
            PlatformUtils.Instance.SetClipboardText(parameter.Name);
        }

        ImGui.BeginDisabled(annotationName.Length == 0);

        if (ImGui.MenuItem($"{LOC.Get("TAE_PropertiesWindow_Field_Copy_Annotation_Name")}##{key}_copyAnnotationName"))
        {
            PlatformUtils.Instance.SetClipboardText(annotationName);
        }

        ImGui.EndDisabled();

        ImGui.EndPopup();
    }

    private string BeginField(string name, string key, string value)
    {
        BeginRow(name);

        return FieldText.TryGetValue(key, out var text) ? text : value;
    }
}
