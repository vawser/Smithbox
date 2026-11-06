using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors;

/// <summary>
/// Lays out the tools of a Tools window as a navigation list, grouped by category, beside a pane that shows the
/// selected tool. When the window is narrow (or the dropdown option is set) the list becomes a combo at the top.
/// <para>
/// Tools draw their headers with <see cref="Header"/>, which returns true only for the selected tool. The window
/// sets the category of the tools that follow with <see cref="Category"/>. The tools register themselves as they
/// are drawn, so the list shows the tools drawn on the previous frame. Outside of a Begin/End pair
/// <see cref="Header"/> acts as a normal collapsing header.
/// </para>
/// </summary>
public class ToolSections
{
    private static ToolSections Current;

    private record struct Entry(string Label, string Name, string Category);

    // The tools drawn this frame, and those drawn last frame, which the navigation shows
    private List<Entry> Entries = new();
    private List<Entry> LastEntries = new();

    private string CurrentCategory;

    // The label of the selected tool. Null until a tool claims it.
    private string Selected;
    private bool SelectedDrawn;

    // Set by Focus, so the next Header becomes the selected tool
    private bool FocusNext;

    private bool ContentOpen;

    // Below this width the navigation is drawn as a combo, so the tool keeps the width it needs
    private const float SideListMinWindowWidth = 420f;

    public void Begin()
    {
        Current = this;

        (LastEntries, Entries) = (Entries, LastEntries);
        Entries.Clear();

        CurrentCategory = null;
        SelectedDrawn = false;
        FocusNext = false;

        // A single tool needs no navigation
        if (LastEntries.Count < 2)
        {
            ContentOpen = false;
            return;
        }

        var scale = DPI.UIScale();
        var useDropdown = CFG.Current.Interface_Tool_Window_Dropdown_Navigation
            || ImGui.GetContentRegionAvail().X < SideListMinWindowWidth * scale;

        if (useDropdown)
        {
            DisplayDropdown();
        }
        else
        {
            DisplaySideList(scale);
            ImGui.SameLine();
        }

        ImGui.BeginChild($"##toolSectionContent_{Selected}", Vector2.Zero);
        ContentOpen = true;
    }

    public void End()
    {
        if (ContentOpen)
        {
            ImGui.EndChild();
            ContentOpen = false;
        }

        Current = null;

        // The selected tool is no longer drawn, so let the first tool claim the selection next frame
        if (!SelectedDrawn)
        {
            Selected = null;
        }
    }

    /// <summary>
    /// The Options menu for the menubar of a Tools window.
    /// </summary>
    public static void OptionsMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_Options")}##toolOptionsMenuHeader"))
        {
            ImGui.Checkbox($"{LOC.Get("PREF_Interface_Tool_Window_Dropdown_Navigation")}##toolDropdownNavigation", ref CFG.Current.Interface_Tool_Window_Dropdown_Navigation);
            GUI.Tooltip(LOC.Get("PREF_Interface_Tool_Window_Dropdown_Navigation_TT"));

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Sets the category that the tools drawn after this call are listed under. The category is a display
    /// name, so pass it through LOC.
    /// </summary>
    public static void Category(string name)
    {
        if (Current != null)
        {
            Current.CurrentCategory = name;
        }
    }

    /// <summary>
    /// Selects the tool whose header is drawn next, as a request to open it does.
    /// </summary>
    public static void Focus()
    {
        if (Current == null)
        {
            ImGui.SetNextItemOpen(true);
            return;
        }

        Current.FocusNext = true;
    }

    public static bool Header(string label)
    {
        if (Current == null)
        {
            return ImGui.CollapsingHeader(label);
        }

        return Current.Register(label);
    }

    private bool Register(string label)
    {
        Entries.Add(new Entry(label, GetName(label), CurrentCategory));

        if (FocusNext)
        {
            FocusNext = false;
            Selected = label;
        }

        // The first tool takes the selection when none is selected
        Selected ??= label;

        if (Selected != label)
        {
            return false;
        }

        SelectedDrawn = true;

        // Without navigation, the first frame has no content child, so the tool still gets drawn
        return true;
    }

    /// <summary>
    /// The visible part of an ImGui label
    /// </summary>
    private static string GetName(string label)
    {
        var index = label.IndexOf("##");
        return index < 0 ? label : label.Substring(0, index);
    }

    /// <summary>
    /// The tools grouped by category, each category in the order its first tool was drawn
    /// </summary>
    private IEnumerable<IGrouping<string, Entry>> GetGroups()
    {
        return LastEntries.GroupBy(e => e.Category);
    }

    private void DisplaySideList(float scale)
    {
        // Fit the longest name, within limits, so the tool keeps most of the width
        var style = ImGui.GetStyle();
        var longest = LastEntries
            .Select(e => e.Name)
            .Concat(LastEntries.Select(e => e.Category ?? ""))
            .Max(n => ImGui.CalcTextSize(n).X);

        var width = longest + (style.WindowPadding.X + style.ItemSpacing.X) * 2 + style.ScrollbarSize;
        width = System.Math.Clamp(width, 120f * scale, 220f * scale);

        ImGui.BeginChild("##toolSectionNavigation", new Vector2(width, 0), ImGuiChildFlags.Borders);

        var first = true;

        foreach (var group in GetGroups())
        {
            if (group.Key != null)
            {
                if (!first)
                {
                    ImGui.Spacing();
                }

                ImGui.SeparatorText(group.Key);
            }

            first = false;

            foreach (var entry in group)
            {
                DisplayEntry(entry, indent: group.Key != null);
            }
        }

        ImGui.EndChild();
    }

    private void DisplayDropdown()
    {
        var selected = LastEntries.FirstOrDefault(e => e.Label == Selected);

        var preview = selected.Label == null ? ""
            : selected.Category == null ? selected.Name
            : $"{selected.Category}: {selected.Name}";

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##toolSectionDropdown", preview, ImGuiComboFlags.HeightLargest))
        {
            foreach (var group in GetGroups())
            {
                if (group.Key != null)
                {
                    ImGui.SeparatorText(group.Key);
                }

                foreach (var entry in group)
                {
                    DisplayEntry(entry, indent: group.Key != null);
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EDITOR_ToolSections_Dropdown_TT"));
    }

    private void DisplayEntry(Entry entry, bool indent)
    {
        if (indent)
        {
            ImGui.Indent();
        }

        if (ImGui.Selectable($"{entry.Name}##toolSectionEntry_{entry.Label}", entry.Label == Selected))
        {
            Selected = entry.Label;
        }

        if (indent)
        {
            ImGui.Unindent();
        }
    }
}
