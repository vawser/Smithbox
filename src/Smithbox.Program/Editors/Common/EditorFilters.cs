using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.Common;

public static class EditorFilters
{
    public static string SplitChr = "+";
    public static string PartSplitChr = "_";
    public static string AliasSplitChr = " ";

    public static string OR_Chr = "|";

    public static Vector2 GetHeaderSize()
    {
        return new Vector2(0, 40) * DPI.UIScale();
    }

    public static void DisplayFramedListFilter(string id, ref string input, ref bool exactBool)
    {
        ImGui.BeginChild($"framedListFilter_{id}", GetHeaderSize(), ImGuiChildFlags.Borders);

        DisplaySearchbar(id, ref input, ref exactBool);
        ImGui.EndChild();
    }

    public static void DisplaySearchbar(string id, ref string input, ref bool exactBool)
    {
        ImGui.Checkbox($"##{id}_listFilter_exactMatch", ref exactBool);
        GUI.Tooltip(
            LOC.Get("EDITOR_List_Filter_Exact_Match_TT"));

        ImGui.SameLine();

        DisplaySearchbarSizeToggle();
        ApplySearchbarWidth();

        ImGui.InputTextWithHint($"##{id}_listFilter", "Search...", ref input, 255);

        GUI.Tooltip(
        LOC.Get("EDITOR_List_Filter_Input_TT", SplitChr, OR_Chr));
    }

    /// <summary>
    /// The arrow that switches search bars between collapsed (a short fixed width) and stretched, followed by
    /// SameLine. Drawn only while the "Display Searchbar Minimize" preference is on. The setting is shared, so every
    /// search bar that uses this follows the same mode. Call just before the search input, then
    /// <see cref="ApplySearchbarWidth"/>.
    /// </summary>
    public static void DisplaySearchbarSizeToggle()
    {
        if (!CFG.Current.Interface.Interface_Display_Searchbar_Minimize)
            return;

        if (CFG.Current.Interface.Interface_Searchbar_Fixed_Width)
        {
            if (ImGui.ArrowButton("##searchMaximize", ImGuiDir.Right))
            {
                CFG.Current.Interface.Interface_Searchbar_Fixed_Width = false;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Search_Size_Toggle_TT"));
        }
        else
        {
            if (ImGui.ArrowButton("##searchMinimize", ImGuiDir.Left))
            {
                CFG.Current.Interface.Interface_Searchbar_Fixed_Width = true;
            }
            GUI.Tooltip(LOC.Get("EDITOR_Search_Size_Toggle_TT"));
        }

        ImGui.SameLine();
    }

    /// <summary>
    /// Sets the next item (the search input) to the collapsed width when search bars are collapsed. Stretched bars
    /// keep the default width.
    /// </summary>
    public static void ApplySearchbarWidth()
    {
        // Only use the fixed width while the toggle is shown, so hiding the
        // buttons can never leave the searchbar stuck at the small size.
        if (CFG.Current.Interface.Interface_Display_Searchbar_Minimize
            && CFG.Current.Interface.Interface_Searchbar_Fixed_Width)
            ImGui.SetNextItemWidth(80f * DPI.UIScale());
    }

    /// <summary>The search terms that match an item's vanilla state instead of its text (lists with vanilla colours)</summary>
    public const string ModifiedTerm = "modified";
    public const string AddedTerm = "added";

    /// <summary>
    /// <see cref="IsMatch(string, string, bool, string, bool, bool, string)" /> for a list that colours its items by
    /// vanilla state: a term that is exactly "modified" or "added" matches items of that state, and combines with the
    /// rest as usual (modified+c1000, added|modified). <paramref name="status" /> is only read for those terms, so it
    /// may be computed lazily.
    /// </summary>
    public static bool IsMatch(string rawInput, string rawText, bool exactBool, Func<VanillaStatus> status,
        string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "")
    {
        if (string.IsNullOrEmpty(rawInput) || rawText == null)
            return true;

        var input = rawInput.Trim().ToLower();
        var terms = input.Contains(OR_Chr) ? input.Split(OR_Chr).ToList() : new List<string> { input };

        VanillaStatus? known = null;
        VanillaStatus Status() => known ??= status?.Invoke() ?? VanillaStatus.None;

        foreach (var term in terms)
        {
            if (IsMatchInternal(term, rawText, exactBool, rawAliasText, partSplit, aliasSplit, rawSecondaryText, Status))
                return true;
        }

        return false;
    }

    /// <summary>Whether a search contains a vanilla-state term, for lists that skip work when nothing asks for it</summary>
    public static bool HasStatusTerm(string rawInput)
    {
        if (string.IsNullOrEmpty(rawInput))
            return false;

        foreach (var part in rawInput.Trim().ToLower().Split(OR_Chr).SelectMany(t => t.Split(SplitChr)))
        {
            var p = part.Trim();
            if (p == ModifiedTerm || p == AddedTerm)
                return true;
        }

        return false;
    }

    public static bool IsMatch(string rawInput, string rawText, bool exactBool, string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "")
    {
        bool isValid = false;

        if (rawInput == null)
            return true;

        if (rawText == null)
            return true;

        if (rawInput == "")
            return true;

        // Allows for multiple terms to be searched for, and returns valid if ANY term is true.
        var input = rawInput.Trim().ToLower();
        List<string> terms = new() { input };

        if (input.Contains(OR_Chr))
        {
            terms = input.Split(OR_Chr).ToList();
        }

        foreach(var term in terms)
        {
            if(IsMatchInternal(term, rawText, exactBool, rawAliasText, partSplit, aliasSplit, rawSecondaryText))
            {
                isValid = true;
            }
        }

        return isValid;
    }

    public static bool IsMatchInternal(string rawInput, string rawText, bool exactBool, string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "", Func<VanillaStatus> status = null)
    {
        bool isValid = true;

        if (rawAliasText == null)
            rawAliasText = "";

        var input = rawInput.Trim().ToLower();
        var text = rawText.Trim().ToLower();
        var alias = "";
        var secondary = "";

        if (rawAliasText != "")
            alias = rawAliasText.Trim().ToLower();

        if (rawSecondaryText != "")
            secondary = rawSecondaryText.Trim().ToLower();

        string[] inputParts = input.Split(SplitChr);
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            // Vanilla state (only for lists that pass one): the term matches the state, never the text
            if (status != null && (entry.Trim() == ModifiedTerm || entry.Trim() == AddedTerm))
            {
                var s = status();
                partTruth[i] = entry.Trim() == ModifiedTerm ? s == VanillaStatus.Modified : s == VanillaStatus.Added;
                continue;
            }

            // Exact
            if (entry == text)
                partTruth[i] = true;

            // Partial
            if (!exactBool)
            {
                if (text.Contains(entry))
                    partTruth[i] = true;
            }

            // Alias
            if(rawAliasText != "")
            {
                if (entry == alias)
                    partTruth[i] = true;

                // Partial
                if (!exactBool)
                {
                    if (alias.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Secondary
            if (rawSecondaryText != "")
            {
                if (entry == secondary)
                    partTruth[i] = true;

                // Partial
                if (!exactBool)
                {
                    if (secondary.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Part Split
            if (partSplit)
            {
                var refParts = text.Split(PartSplitChr);
                foreach (var refPart in refParts)
                {
                    if (entry == refPart)
                        partTruth[i] = true;

                    if (!exactBool)
                    {
                        if (refPart.Contains(entry))
                            partTruth[i] = true;
                    }
                }
            }

            // Alias Split
            if (aliasSplit)
            {
                var refParts = alias.Split(AliasSplitChr);
                foreach (var refPart in refParts)
                {
                    if (entry == refPart)
                        partTruth[i] = true;

                    if (!exactBool)
                    {
                        if (refPart.Contains(entry))
                            partTruth[i] = true;
                    }
                }
            }
        }

        // Only evaluate as true if all parts are true
        foreach (bool entry in partTruth)
        {
            if (!entry)
                isValid = false;
        }

        return isValid;
    }
}