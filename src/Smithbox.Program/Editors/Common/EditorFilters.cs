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

    /// <param name="exactTooltipKey">The Exact checkbox's tooltip, for a list where exact means something more
    /// specific than "no partial matches"</param>
    public static void DisplaySearchbar(string id, ref string input, ref bool exactBool, string exactTooltipKey = null)
    {
        ImGui.Checkbox($"##{id}_listFilter_exactMatch", ref exactBool);
        GUI.Tooltip(
            LOC.Get(exactTooltipKey ?? "EDITOR_List_Filter_Exact_Match_TT"));

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

    /// <summary>A term starting with this matches the items the rest of the term doesn't (!c1000)</summary>
    public const string NotChr = "!";

    /// <summary>
    /// <see cref="IsMatch(string, string, bool, string, bool, bool, string, Func{string, string})" /> for a list that
    /// colours its items by vanilla state: a term that is exactly "modified" or "added" matches items of that state,
    /// and combines with the rest as usual (modified+c1000, added|modified, !modified). <paramref name="status" /> is
    /// only read for those terms, so it may be computed lazily.
    /// </summary>
    public static bool IsMatch(string rawInput, string rawText, bool exactBool, Func<VanillaStatus> status,
        string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "",
        Func<string, string> alternateTerm = null)
    {
        if (string.IsNullOrEmpty(rawInput) || rawText == null)
            return true;

        var input = rawInput.Trim().ToLower();

        VanillaStatus? known = null;
        VanillaStatus Status() => known ??= status?.Invoke() ?? VanillaStatus.None;

        foreach (var term in input.Split(OR_Chr))
        {
            if (IsMatchInternal(term, rawText, exactBool, rawAliasText, partSplit, aliasSplit, rawSecondaryText, Status, alternateTerm))
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
            var p = StripNot(part.Trim(), out _);
            if (p == ModifiedTerm || p == AddedTerm)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Whether the item matches the search. Terms joined by | match if any does; within one, terms joined by +
    /// must all match; a term starting with ! must not. <paramref name="alternateTerm" /> may give a second form
    /// of a term that also counts as a match (null when it has none).
    /// </summary>
    public static bool IsMatch(string rawInput, string rawText, bool exactBool, string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "",
        Func<string, string> alternateTerm = null)
    {
        if (string.IsNullOrEmpty(rawInput) || rawText == null)
            return true;

        var input = rawInput.Trim().ToLower();

        foreach (var term in input.Split(OR_Chr))
        {
            if (IsMatchInternal(term, rawText, exactBool, rawAliasText, partSplit, aliasSplit, rawSecondaryText, null, alternateTerm))
                return true;
        }

        return false;
    }

    /// <summary>The term without a leading !, and whether it had one. A lone ! is kept as text.</summary>
    public static string StripNot(string term, out bool negated)
    {
        negated = term.Length > 1 && term.StartsWith(NotChr);
        return negated ? term.Substring(1).Trim() : term;
    }

    public static bool IsMatchInternal(string rawInput, string rawText, bool exactBool, string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "", Func<VanillaStatus> status = null,
        Func<string, string> alternateTerm = null)
    {
        var input = rawInput.Trim().ToLower();
        var text = rawText.Trim().ToLower();
        var alias = string.IsNullOrEmpty(rawAliasText) ? "" : rawAliasText.Trim().ToLower();
        var secondary = string.IsNullOrEmpty(rawSecondaryText) ? "" : rawSecondaryText.Trim().ToLower();

        foreach (var part in input.Split(SplitChr))
        {
            // Trimmed, so "a + b" searches for "a" and "b" rather than "a " and " b"
            var entry = StripNot(part.Trim(), out var negated);

            bool isMatch;

            // Vanilla state (only for lists that pass one): the term matches the state, never the text
            if (status != null && (entry == ModifiedTerm || entry == AddedTerm))
            {
                var s = status();
                isMatch = entry == ModifiedTerm ? s == VanillaStatus.Modified : s == VanillaStatus.Added;
            }
            else
            {
                isMatch = IsTermMatch(entry, text, alias, secondary, exactBool, partSplit, aliasSplit);

                if (!isMatch && alternateTerm != null)
                {
                    var alternate = alternateTerm(entry);

                    if (alternate != null && alternate != entry)
                        isMatch = IsTermMatch(alternate, text, alias, secondary, exactBool, partSplit, aliasSplit);
                }
            }

            // Every term must hold
            if (isMatch == negated)
                return false;
        }

        return true;
    }

    /// <summary>
    /// One search term against the item's text, alias and secondary text: equal to any of them, or (unless exact)
    /// contained in one; with the splits, equal to or contained in one _ part of the text or one word of the alias
    /// </summary>
    private static bool IsTermMatch(string entry, string text, string alias, string secondary, bool exactBool, bool partSplit, bool aliasSplit)
    {
        if (IsTextMatch(entry, text, exactBool))
            return true;

        if (alias != "" && IsTextMatch(entry, alias, exactBool))
            return true;

        if (secondary != "" && IsTextMatch(entry, secondary, exactBool))
            return true;

        if (partSplit && text.Split(PartSplitChr).Any(p => IsTextMatch(entry, p, exactBool)))
            return true;

        if (aliasSplit && alias.Split(AliasSplitChr).Any(p => IsTextMatch(entry, p, exactBool)))
            return true;

        return false;
    }

    private static bool IsTextMatch(string entry, string text, bool exactBool)
    {
        return entry == text || (!exactBool && text.Contains(entry));
    }
}
