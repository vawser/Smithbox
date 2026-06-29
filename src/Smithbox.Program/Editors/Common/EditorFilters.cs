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

    public static void DisplayFramedListFilter(string id, ref string input, ref bool exactBool)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_{id}", searchHeight, ImGuiChildFlags.Borders);

        DisplayListFilter(id, ref input, ref exactBool);
        ImGui.EndChild();
    }

    public static void DisplayListFilter(string id, ref string input, ref bool exactBool)
    {
        ImGui.Checkbox($"##{id}_listFilter_exactMatch", ref exactBool);
        UIHelper.Tooltip(
            LOC.Get("EDITOR_List_Filter_Exact_Match_TT"));

        ImGui.SameLine();

        ImGui.InputTextWithHint($"##{id}_listFilter", "Search...", ref input, 255);
        UIHelper.Tooltip(
            LOC.Get("EDITOR_List_Filter_Input_TT", SplitChr, OR_Chr));
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

    public static bool IsMatchInternal(string rawInput, string rawText, bool exactBool, string rawAliasText = "", bool partSplit = false, bool aliasSplit = false, string rawSecondaryText = "")
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