using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
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

    // How to apply the text typed into each field, kept while it is being typed. Clicking another event to select
    // it takes the focus off the field in the same frame the selection changes, so the text would otherwise be
    // lost: it is applied to what it was typed for before the fields switch over.
    private Dictionary<string, Action> PendingEventCommits = new();
    private Dictionary<string, Action> PendingTimeActCommits = new();

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

        CommitOnTargetChange(entry, animation, ev);

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

    /// <summary>
    /// The typed text belongs to what it was typed for. When the selection moves on, any text still being typed
    /// is applied to that, as leaving a field does, and the fields start over with the new selection's values.
    /// </summary>
    private void CommitOnTargetChange(TimeActEntry entry, TAE.Animation animation, TAE.Event ev)
    {
        if (FieldTextEvent != ev)
        {
            RunPendingCommits(PendingEventCommits);

            FieldText.Clear();
            FieldTextEvent = ev;
        }

        if (TimeActFieldTextEntry != entry || TimeActFieldTextAnimation != animation)
        {
            RunPendingCommits(PendingTimeActCommits);

            TimeActFieldText.Clear();
            TimeActFieldTextEntry = entry;
            TimeActFieldTextAnimation = animation;
        }
    }

    private static void RunPendingCommits(Dictionary<string, Action> commits)
    {
        var pending = commits.Values.ToList();

        commits.Clear();

        foreach (var commit in pending)
        {
            commit();
        }
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
            // Vanilla
            DisplayEventVanillaRow(ev);

            // Each event keeps its own other end, so setting the start of several doesn't line up their ends
            DisplayTimeField("TAE_PropertiesWindow_Start_Time", ev.StartTime, (target, value) => (value, target.EndTime));
            DisplayTimeField("TAE_PropertiesWindow_End_Time", ev.EndTime, (target, value) => (target.StartTime, value));

            DisplayUnk04Field(ev);
            DisplayEventGroup(ev);

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
            ImGui.BeginDisabled(!Project.Handler.TimeActData.CanSaveTimeActs);

            DisplayRawParameters(ev);

            ImGui.EndDisabled();
        }
    }

    private const int RawBytesPerRow = 16;

    // Working out why the template doesn't fit reads the event through it, which is kept from frame to frame
    private (TAE.Event Event, string Bytes, string Reason) RawMismatchCache;

    /// <summary>
    /// The parameters of an event no template fits, as rows of hex bytes. Each row is edited on its own, so the
    /// event keeps its length: the game reads a set size for each type. Selected events of the same type that
    /// have no template either, and the same length, get the edited row as well.
    /// </summary>
    private void DisplayRawParameters(TAE.Event ev)
    {
        var entry = View.Selection.SelectedTimeAct;
        var animation = View.Selection.SelectedAnimation;

        if (entry?.TimeAct == null || animation == null)
            return;

        var bigEndian = entry.TimeAct.BigEndian;
        var bytes = ev.GetParameterBytes(bigEndian);
        var hex = Convert.ToHexString(bytes);

        GUI.Spacer();
        GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_No_Template", ev.Type));

        var template = Project.Handler.TimeActData.GetTemplate(View.Selection.SelectedTimeActContainerWrapper);

        if (RawMismatchCache.Event != ev || RawMismatchCache.Bytes != hex)
        {
            RawMismatchCache = (ev, hex, entry.DescribeTemplateMismatch(ev, template));
        }

        if (RawMismatchCache.Reason is { } reason)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, ImGui.GetStyle().Colors[(int)ImGuiCol.TextDisabled]);
            GUI.WrappedText(reason);
            ImGui.PopStyleColor();
        }

        if (bytes.Length == 0)
        {
            ImGui.TextDisabled(LOC.Get("TAE_PropertiesWindow_Raw_Empty"));
            return;
        }

        // Taken now, as the selection may have moved on by the time a pending edit is applied
        var length = bytes.Length;
        var targets = (EventTargets.Count > 0 ? EventTargets : [ev])
            .Where(e => e.Parameters == null && e.GetParameterBytes(bigEndian).Length == length)
            .ToList();

        if (!targets.Contains(ev))
        {
            targets.Insert(0, ev);
        }

        if (!ImGui.BeginTable("##rawParameters", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
            return;

        ImGui.TableSetupColumn("##rawOffset", ImGuiTableColumnFlags.WidthFixed, ImGui.CalcTextSize("+0000").X + 8);
        ImGui.TableSetupColumn("##rawBytes", ImGuiTableColumnFlags.WidthStretch);

        for (var offset = 0; offset < length; offset += RawBytesPerRow)
        {
            var rowOffset = offset;
            var count = Math.Min(RawBytesPerRow, length - offset);
            var key = $"raw_{offset}";

            ImGui.TableNextRow();

            ImGui.TableSetColumnIndex(0);
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled($"+{offset:X2}");

            ImGui.TableSetColumnIndex(1);

            var text = FieldText.TryGetValue(key, out var typed) ? typed : FormatRawRow(bytes, offset, count);

            ImGui.SetNextItemWidth(-1);

            if (ImGui.InputText($"##{key}", ref text, MaxTextLength))
            {
                FieldText[key] = text;
            }

            GUI.Tooltip(DescribeRawRow(bytes, offset, count, bigEndian));

            void Commit(string committed)
            {
                if (!TryParseRawRow(committed, count, out var row))
                {
                    Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_Raw_Invalid", committed, count));
                    return;
                }

                var actions = new List<ViewportAction>();

                // Events deleted since aren't edited
                foreach (var target in targets.Where(e => animation.Events.Contains(e)))
                {
                    var current = target.GetParameterBytes(bigEndian);

                    if (current.Length != length)
                        continue;

                    var changed = current.ToArray();
                    Array.Copy(row, 0, changed, rowOffset, count);

                    if (changed.AsSpan().SequenceEqual(current))
                        continue;

                    actions.Add(new TimeActSetParameterBytesAction(entry, target, changed));
                }

                ExecuteActions(actions);
            }

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                FieldText.Remove(key);
                PendingEventCommits.Remove(key);

                Commit(text);
            }
            else if (FieldText.TryGetValue(key, out var pendingText))
            {
                PendingEventCommits[key] = () => Commit(pendingText);
            }
        }

        ImGui.EndTable();

        // Under the bytes, as it lists them against the template's fields
        if (template != null && template.TryGetValue(ev.Type, out var eventTemplate))
        {
            DisplayForceTemplate(entry, ev, eventTemplate, hex);
        }
    }

    // More changed bytes than this are counted rather than listed
    private const int MaxListedTemplateChanges = 64;

    // What putting the template on anyway would change, kept from frame to frame for the event and bytes on show
    private (TAE.Event Event, string Bytes, bool Readable, List<TimeActEntry.TemplateByteChange> Changes, string Error)
        ForceTemplateCache;

    /// <summary>
    /// For an event whose type has a template that doesn't fit it exactly: lists each byte the template would
    /// write differently, and the field it falls in, and can put the template on anyway. This is for testing
    /// templates in place: the bytes listed are given up when the time act is saved, so it can be undone, but
    /// only until then.
    /// </summary>
    private void DisplayForceTemplate(TimeActEntry entry, TAE.Event ev, TAE.Template.EventTemplate eventTemplate, string hex)
    {
        GUI.Spacer();

        if (!ImGui.CollapsingHeader($"{LOC.Get("TAE_PropertiesWindow_Force_Template_Header", eventTemplate.Name)}##forceTemplate"))
            return;

        if (ForceTemplateCache.Event != ev || ForceTemplateCache.Bytes != hex)
        {
            var readable = entry.TryPreviewForcedTemplate(ev, eventTemplate, out var previewChanges, out var previewError);

            ForceTemplateCache = (ev, hex, readable, previewChanges, previewError);
        }

        if (!ForceTemplateCache.Readable)
        {
            GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_Force_Template_Unreadable", ForceTemplateCache.Error));
            return;
        }

        var changes = ForceTemplateCache.Changes;

        GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_Force_Template_Summary", changes.Count,
            changes.Count(e => e.IsFixedValue), changes.Count(e => e.New == null)));

        if (changes.Count > 0 && ImGui.BeginTable("##forceTemplateChanges", 4,
            ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn(LOC.Get("TAE_PropertiesWindow_Force_Template_Column_Offset"));
            ImGui.TableSetupColumn(LOC.Get("TAE_PropertiesWindow_Force_Template_Column_Field"), ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupColumn(LOC.Get("TAE_PropertiesWindow_Force_Template_Column_File"));
            ImGui.TableSetupColumn(LOC.Get("TAE_PropertiesWindow_Force_Template_Column_Template"));
            ImGui.TableHeadersRow();

            foreach (var change in changes.Take(MaxListedTemplateChanges))
            {
                ImGui.TableNextRow();

                ImGui.TableSetColumnIndex(0);
                ImGui.TextUnformatted($"+{change.Offset:X2}");

                ImGui.TableSetColumnIndex(1);

                if (change.New == null)
                {
                    ImGui.TextDisabled(LOC.Get("TAE_PropertiesWindow_Force_Template_Dropped"));
                }
                else if (change.IsFixedValue)
                {
                    ImGui.TextUnformatted(LOC.Get("TAE_PropertiesWindow_Force_Template_Fixed", change.Field));
                }
                else
                {
                    ImGui.TextUnformatted(Project.Handler.TimeActData.GetFieldDisplayName(eventTemplate, change.Field));
                }

                ImGui.TableSetColumnIndex(2);
                ImGui.TextUnformatted(change.Old is { } oldByte ? $"{oldByte:X2}" : "--");

                ImGui.TableSetColumnIndex(3);
                ImGui.TextUnformatted(change.New is { } newByte ? $"{newByte:X2}" : "--");
            }

            ImGui.EndTable();

            if (changes.Count > MaxListedTemplateChanges)
            {
                ImGui.TextDisabled(LOC.Get("TAE_PropertiesWindow_Force_Template_More", changes.Count - MaxListedTemplateChanges));
            }
        }

        GUI.WrappedText(LOC.Get("TAE_PropertiesWindow_Force_Template_Warning"));

        if (ImGui.Button($"{LOC.Get("TAE_PropertiesWindow_Force_Template_Apply")}##applyTemplateAnyway"))
        {
            View.ViewportActionManager.ExecuteAction(new TimeActForceTemplateAction(entry, ev, eventTemplate));

            FieldText.Clear();
            PendingEventCommits.Clear();

            Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_Force_Template_Applied", eventTemplate.Name, changes.Count));
        }
    }

    private static string FormatRawRow(byte[] bytes, int offset, int count)
    {
        return string.Join(" ", bytes.Skip(offset).Take(count).Select(e => $"{e:X2}"));
    }

    /// <summary>
    /// The row must hold exactly as many bytes as it had, in hex, with or without spaces
    /// </summary>
    private static bool TryParseRawRow(string text, int count, out byte[] row)
    {
        row = null;

        var hex = new string((text ?? "").Where(c => !char.IsWhiteSpace(c)).ToArray());

        if (hex.Length != count * 2)
            return false;

        try
        {
            row = Convert.FromHexString(hex);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// What each four bytes of the row read as, to help make out unknown parameters
    /// </summary>
    private static string DescribeRawRow(byte[] bytes, int offset, int count, bool bigEndian)
    {
        var lines = new List<string> { LOC.Get("TAE_PropertiesWindow_Raw_Row_TT") };

        for (var i = offset; i + 4 <= offset + count; i += 4)
        {
            var span = bytes.AsSpan(i, 4);

            var u32 = bigEndian ? System.Buffers.Binary.BinaryPrimitives.ReadUInt32BigEndian(span)
                : System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(span);

            var f32 = BitConverter.Int32BitsToSingle((int)u32);

            lines.Add($"+{i:X2}: u32 {u32}  s32 {(int)u32}  f32 {f32.ToString(CultureInfo.InvariantCulture)}");
        }

        return string.Join("\n", lines);
    }

    private delegate bool TryParseValue<T>(string text, out T value);

    /// <summary>
    /// The properties of the time act's header. The format can't be changed, as it decides how the rest is laid out.
    /// </summary>
    private void DisplayTimeActProperties(TimeActEntry entry)
    {
        var tae = entry.TimeAct;

        GUI.SimpleHeader(
            LOC.Get("TAE_PropertiesWindow_TimeAct_Header", entry.Name),
            LOC.Get("TAE_PropertiesWindow_TimeAct_Header_TT"));

        if (!ImGui.BeginTable("##timeActProperties", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
            return;

        // Vanilla
        DisplayTimeActVanillaRow(entry);

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

    #region Vanilla
    /// <summary>
    /// The row that says how this compares with the game's version, with what differs in its tooltip and a button
    /// to revert it
    /// </summary>
    private void DisplayVanillaRow(TimeActVanillaStatus status, List<string> details, bool canRevert, Action revert)
    {
        var name = LOC.Get("TAE_PropertiesWindow_Vanilla");

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter))
            return;

        var source = View.Selection.SelectedTimeActContainerWrapper;

        BeginRow(name);

        var text = TimeActVanillaDisplay.GetStatusText(status, source?.Vanilla.IsReading == true);

        if (TimeActVanillaDisplay.GetColor(status) is { } color)
        {
            ImGui.TextColored(color, text);
        }
        else
        {
            ImGui.TextUnformatted(text);
        }

        GUI.Tooltip(details.Count > 0
            ? $"{LOC.Get("TAE_PropertiesWindow_Vanilla_TT")}\n\n{string.Join("\n", details)}"
            : LOC.Get("TAE_PropertiesWindow_Vanilla_TT"));

        if (!canRevert)
            return;

        ImGui.SameLine();

        ImGui.BeginDisabled(!Project.Handler.TimeActData.CanSaveTimeActs);

        if (ImGui.SmallButton($"{LOC.Get("TAE_PropertiesWindow_Vanilla_Revert")}##vanillaRevert"))
        {
            revert();
        }
        GUI.Tooltip(LOC.Get("TAE_PropertiesWindow_Vanilla_Revert_TT"));

        ImGui.EndDisabled();
    }

    private void DisplayTimeActVanillaRow(TimeActEntry entry)
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;
        var diff = source.Vanilla.GetEntryDiff(entry, true);

        var details = new List<string>();

        if (diff.Status == TimeActVanillaStatus.Modified && diff.Detailed)
        {
            if (diff.HeaderChanged)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Header"));

            if (diff.ModifiedAnimations > 0)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Animations_Modified", diff.ModifiedAnimations));

            if (diff.AddedAnimations > 0)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Animations_Added", diff.AddedAnimations));

            if (diff.RemovedAnimations.Count > 0)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Animations_Removed", diff.RemovedAnimations.Count));
        }

        DisplayVanillaRow(diff.Status, details, diff.Status == TimeActVanillaStatus.Modified,
            () => TimeActRevert.RevertTimeAct(View, source, entry));
    }

    private void DisplayAnimationVanillaRow(TimeActEntry entry, TAE.Animation animation)
    {
        var source = View.Selection.SelectedTimeActContainerWrapper;
        var diff = source.Vanilla.GetAnimationDiff(entry, animation);

        var details = new List<string>();

        if (diff.Status == TimeActVanillaStatus.Modified)
        {
            if (diff.MiniHeaderChanged)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Mini_Header"));

            if (diff.FileNameChanged)
                details.Add(LOC.Get("TAE_Vanilla_Detail_File_Name", diff.Vanilla.AnimFileName ?? ""));

            if (diff.GroupsChanged)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Groups"));

            if (diff.EventsChanged && source.Vanilla.GetEventMatch(entry, animation) is { } match)
            {
                if (match.ModifiedCount > 0)
                    details.Add(LOC.Get("TAE_Vanilla_Detail_Events_Modified", match.ModifiedCount));

                if (match.Added.Count > 0)
                    details.Add(LOC.Get("TAE_Vanilla_Detail_Events_Added", match.Added.Count));

                if (match.Removed.Count > 0)
                    details.Add(LOC.Get("TAE_Vanilla_Detail_Events_Removed", match.Removed.Count));
            }
        }

        DisplayVanillaRow(diff.Status, details, diff.Status == TimeActVanillaStatus.Modified,
            () => TimeActRevert.RevertAnimation(View, source, entry, animation));
    }

    /// <summary>
    /// The primary event, compared with the game's event it matches. The changed values are listed with the game's
    /// value, by parameter name where both have a template, or else by how many bytes differ.
    /// </summary>
    private void DisplayEventVanillaRow(TAE.Event ev)
    {
        var selection = View.Selection;
        var source = selection.SelectedTimeActContainerWrapper;
        var entry = selection.SelectedTimeAct;
        var animation = selection.SelectedAnimation;

        if (source == null || entry?.TimeAct == null || animation == null)
            return;

        var match = source.Vanilla.GetEventMatch(entry, animation);

        var status = match?.GetStatus(ev) ?? TimeActVanillaStatus.Unknown;
        var details = new List<string>();

        if (status == TimeActVanillaStatus.Modified)
        {
            var vanilla = match.Matches[ev];
            var frameDuration = View.Playback.FrameDuration;

            if (vanilla.StartTime != ev.StartTime)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Value", LOC.Get("TAE_PropertiesWindow_Start_Time"),
                    FormatTime(vanilla.StartTime, frameDuration)));

            if (vanilla.EndTime != ev.EndTime)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Value", LOC.Get("TAE_PropertiesWindow_End_Time"),
                    FormatTime(vanilla.EndTime, frameDuration)));

            if (vanilla.Unk04 != ev.Unk04)
                details.Add(LOC.Get("TAE_Vanilla_Detail_Value", "Unk04", $"{vanilla.Unk04}"));

            AddParameterDetails(source, entry, ev, vanilla, details);
        }

        // A selection of several events reverts each of them that the game has
        DisplayVanillaRow(status, details, status == TimeActVanillaStatus.Modified,
            () => TimeActRevert.RevertEvents(View, source, entry, animation,
                selection.SelectedEvents.Count > 0 ? selection.SelectedEvents.ToList() : [ev]));
    }

    private void AddParameterDetails(TimeActContainerWrapper source, TimeActEntry entry, TAE.Event ev, TAE.Event vanilla,
        List<string> details)
    {
        var bigEndian = entry.TimeAct.BigEndian;

        var currentBytes = ev.GetParameterBytes(bigEndian);
        var vanillaBytes = vanilla.GetParameterBytes(bigEndian);

        if (TimeActCompare.ParametersEqual(currentBytes, vanillaBytes))
            return;

        var template = Project.Handler.TimeActData.GetTemplate(source);
        var vanillaEntry = source.Vanilla.GetEntryDiff(entry).Vanilla;

        var readable = ev.Parameters != null && vanillaEntry?.TimeAct != null
            ? vanillaEntry.GetReadableEvent(vanilla, template)
            : null;

        if (readable?.Parameters != null && readable.Template == ev.Template)
        {
            foreach (var (key, parameter) in ev.Parameters.Template)
            {
                if (parameter.ValueToAssert != null)
                    continue;

                var vanillaValue = TimeActAction.FormatValue(readable.Parameters[key]);

                if (vanillaValue != TimeActAction.FormatValue(ev.Parameters[key]))
                {
                    details.Add(LOC.Get("TAE_Vanilla_Detail_Value", parameter.Name, vanillaValue));
                }
            }

            return;
        }

        var differing = 0;

        for (int i = 0; i < Math.Max(currentBytes.Length, vanillaBytes.Length); i++)
        {
            var a = i < currentBytes.Length ? currentBytes[i] : 0;
            var b = i < vanillaBytes.Length ? vanillaBytes[i] : 0;

            if (a != b)
                differing++;
        }

        details.Add(LOC.Get("TAE_Vanilla_Detail_Bytes", differing));
    }
    #endregion

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

        // Taken now, as a pending edit is applied once the animation tab has moved on
        var animation = PropertyAnimation;

        void Commit(string committed)
        {
            if (!parse(committed, out var newValue))
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_TimeAct_Invalid_Value", committed, name));
                return;
            }

            if (format(newValue) == valueText)
                return;

            if (validate?.Invoke(newValue) is { } error)
            {
                Smithbox.Log(this, error);
                return;
            }

            View.ViewportActionManager.ExecuteAction(new TimeActSetPropertyAction<T>(entry, set, value, newValue, onChanged,
                name, animation));
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            TimeActFieldText.Remove(nameKey);
            PendingTimeActCommits.Remove(nameKey);

            Commit(text);
        }
        else if (TimeActFieldText.TryGetValue(nameKey, out var pendingText))
        {
            // Applied to this time act or animation if the selection moves on before the field is left
            PendingTimeActCommits[nameKey] = () => Commit(pendingText);
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
            View.ViewportActionManager.ExecuteAction(new TimeActSetPropertyAction<bool>(entry, set, value, ticked, onChanged,
                name, PropertyAnimation));
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

        GUI.SimpleHeader(
            LOC.Get("TAE_PropertiesWindow_Animation_Header", source.GetAnimationName(entry, animation)),
            LOC.Get("TAE_PropertiesWindow_Animation_Header_TT"));

        if (!ImGui.BeginTable("##animationProperties", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.BordersInnerV))
            return;

        // The edits made in this tab belong to the animation, so the history can go back to it
        PropertyAnimation = animation;

        // Vanilla
        DisplayAnimationVanillaRow(entry, animation);

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

        PropertyAnimation = null;
    }

    // The animation whose tab is being drawn, which its property edits belong to; null in the time act tab
    private TAE.Animation PropertyAnimation;

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
                        () => OnAnimationMotionChanged(animation), name, animation));
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

    /// <summary>
    /// A time in seconds, with the frame it falls on after it, as "0.43333334 (13f)". Frames that don't come out
    /// whole get two decimals. The placeholder of an event that lasts until the end of the animation says so.
    /// </summary>
    private static string FormatTime(float seconds, float frameDuration)
    {
        var text = seconds.ToString(CultureInfo.InvariantCulture);

        if (seconds >= TAE.Event.EldenRingInfiniteLengthEventPlaceholder)
            return $"{text} ({LOC.Get("TAE_PropertiesWindow_Time_To_End")})";

        var frames = seconds / frameDuration;
        var whole = MathF.Round(frames);

        var frameText = MathF.Abs(frames - whole) < 0.001f
            ? whole.ToString(CultureInfo.InvariantCulture)
            : MathF.Round(frames, 2).ToString(CultureInfo.InvariantCulture);

        return $"{text} ({frameText}f)";
    }

    /// <summary>
    /// Reads a time typed as seconds ("0.5") or as frames ("15f"). Anything in brackets is left out, so the text
    /// shown can be edited in place: "0.5 (13f)" is read as 0.5 seconds.
    /// </summary>
    private static bool TryParseTime(string text, float frameDuration, out float seconds)
    {
        seconds = 0;

        var value = (text ?? "").Trim();

        var bracket = value.IndexOf('(');

        if (bracket >= 0)
        {
            value = value.Substring(0, bracket).Trim();
        }

        if (value.EndsWith('f') || value.EndsWith('F'))
        {
            if (!float.TryParse(value[..^1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var frames))
                return false;

            seconds = frames * frameDuration;
            return true;
        }

        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out seconds);
    }

    /// <summary>
    /// The unknown value every event has after its times. New events take it from another event of their type,
    /// as its meaning isn't known.
    /// </summary>
    private void DisplayUnk04Field(TAE.Event ev)
    {
        const string nameKey = "TAE_PropertiesWindow_Unk04";

        var name = LOC.Get(nameKey);

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter, "Unk04"))
            return;

        var text = BeginField(name, nameKey, $"{ev.Unk04}");

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{nameKey}", ref text, MaxTextLength))
        {
            FieldText[nameKey] = text;
        }

        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));

        // Taken now, as the selection may have moved on by the time a pending edit is applied
        var targets = EventTargets.ToList();
        var entry = View.Selection.SelectedTimeAct;
        var animation = View.Selection.SelectedAnimation;

        void Commit(string committed)
        {
            if (!int.TryParse(committed.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var newValue))
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_TimeAct_Invalid_Value", committed, name));
                return;
            }

            var actions = new List<ViewportAction>();

            foreach (var target in targets.Where(e => animation?.Events.Contains(e) == true && e.Unk04 != newValue))
            {
                actions.Add(new TimeActSetPropertyAction<int>(entry, value => target.Unk04 = value, target.Unk04, newValue,
                    null, name, animation));
            }

            ExecuteActions(actions);
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(nameKey);
            PendingEventCommits.Remove(nameKey);

            Commit(text);
        }
        else if (FieldText.TryGetValue(nameKey, out var pendingText))
        {
            PendingEventCommits[nameKey] = () => Commit(pendingText);
        }
    }

    /// <summary>
    /// The event group the event is in, read only: what group types and data mean isn't known well enough to
    /// edit them safely. Animations without groups (the older formats) show nothing.
    /// </summary>
    private void DisplayEventGroup(TAE.Event ev)
    {
        var animation = View.Selection.SelectedAnimation;

        if (animation == null || animation.EventGroups.Count == 0)
            return;

        var name = LOC.Get("TAE_PropertiesWindow_Event_Group");

        if (!EditorFilters.IsMatch(PropertiesFilter, name, ExactPropertiesFilter))
            return;

        BeginRow(name);

        var group = ev.Group;

        if (group == null || !animation.EventGroups.Contains(group))
        {
            ImGui.TextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("TAE_PropertiesWindow_Event_Group_None"));
            GUI.Tooltip(LOC.Get("TAE_PropertiesWindow_Event_Group_None_TT"));
            return;
        }

        var data = group.GroupData;
        var members = animation.Events.Count(e => e.Group == group);

        ImGui.TextUnformatted(LOC.Get("TAE_PropertiesWindow_Event_Group_Value", group.GroupType, (long)data.DataType, members));

        var lines = new List<string>
        {
            LOC.Get("TAE_PropertiesWindow_Event_Group_TT"),
            LOC.Get("TAE_PropertiesWindow_Event_Group_Data_Type", data.DataType)
        };

        if (data.DataType is TAE.EventGroup.EventGroupDataType.ApplyToSpecificCutsceneEntity)
        {
            lines.Add(LOC.Get("TAE_PropertiesWindow_Event_Group_Cutscene", data.CutsceneEntityType,
                data.CutsceneEntityIDPart1, data.CutsceneEntityIDPart2, data.Area, data.Block));
        }

        GUI.Tooltip(string.Join("\n", lines));
    }

    private void DisplayTimeField(string nameKey, float value, Func<TAE.Event, float, (float Start, float End)> apply)
    {
        var frameDuration = View.Playback.FrameDuration;
        var name = LOC.Get(nameKey);

        var text = BeginField(name, nameKey, FormatTime(value, frameDuration));

        ImGui.SetNextItemWidth(-1);

        if (ImGui.InputText($"##{nameKey}", ref text, MaxTextLength))
        {
            FieldText[nameKey] = text;
        }

        GUI.Tooltip(LOC.Get("TAE_PropertiesWindow_Time_TT"));

        // Taken now, as the selection may have moved on by the time a pending edit is applied
        var targets = EventTargets.ToList();
        var entry = View.Selection.SelectedTimeAct;
        var animation = View.Selection.SelectedAnimation;

        void Commit(string committed)
        {
            if (!TryParseTime(committed, frameDuration, out var newValue))
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_Invalid_Time", committed, name));
                return;
            }

            var actions = new List<ViewportAction>();

            // Events deleted since aren't edited
            foreach (var target in targets.Where(e => animation?.Events.Contains(e) == true))
            {
                var (start, end) = apply(target, newValue);

                if (start == target.StartTime && end == target.EndTime)
                    continue;

                actions.Add(new TimeActMoveEventAction(entry, target, target.StartTime, target.EndTime, start, end));
            }

            ExecuteActions(actions);
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(nameKey);
            PendingEventCommits.Remove(nameKey);

            Commit(text);
        }
        else if (FieldText.TryGetValue(nameKey, out var pendingText))
        {
            PendingEventCommits[nameKey] = () => Commit(pendingText);
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

        // Taken now, as the selection may have moved on by the time a pending edit is applied
        var targets = (EventTargets.Count > 0 ? EventTargets : [ev]).ToList();
        var entry = View.Selection.SelectedTimeAct;
        var animation = View.Selection.SelectedAnimation;

        void Commit(string committed)
        {
            try
            {
                ApplyParameter(targets.Where(e => animation?.Events.Contains(e) == true), entry, key,
                    parameter.StringToValue(committed));
            }
            catch (Exception e) when (e is FormatException or OverflowException or IndexOutOfRangeException)
            {
                Smithbox.Log(this, LOC.Get("TAE_PropertiesWindow_Invalid_Value", committed, parameter.Type));
            }
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            FieldText.Remove(key);
            PendingEventCommits.Remove(key);

            Commit(text);
        }
        else if (FieldText.TryGetValue(key, out var pendingText))
        {
            PendingEventCommits[key] = () => Commit(pendingText);
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

        // An effect id: Ctrl+click, or the context menu, opens it in the FX Editor (the alias list may not name it)
        bool isEffect = aliasType is ProjectAliasType.Particles;
        if (isEffect)
            FxReferenceHelper.Click(Project, value);

        if (ImGui.BeginPopupContextItem("##aliasContextMenu"))
        {
            if (isEffect && FxReferenceHelper.IsAvailable(Project) && FxReferenceHelper.TryGetId(value, out _))
            {
                FxReferenceHelper.ContextMenu(Project, value);
                ImGui.Separator();
            }

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
        ApplyParameter(EventTargets.Count > 0 ? EventTargets : [ev], View.Selection.SelectedTimeAct, key, value);
    }

    /// <summary>
    /// Sets the parameter on the events given, which belong to the time act given
    /// </summary>
    private void ApplyParameter(IEnumerable<TAE.Event> targets, TimeActEntry entry, string key, object value)
    {
        var actions = new List<ViewportAction>();

        foreach (var target in targets)
        {
            if (target.Parameters == null || !target.Parameters.Values.ContainsKey(key))
                continue;

            if (value.Equals(target.Parameters[key]))
                continue;

            actions.Add(new TimeActSetParameterAction(entry, target, key, value));
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
            View.ViewportActionManager.ExecuteAction(new TimeActCompoundAction(actions));
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
