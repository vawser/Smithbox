
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Interface;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class TextFilters
{
    private TextEditorScreen Editor;
    private TextPropertyDecorator Decorator;
    private TextSelectionManager Selection;

    public TextFilters(TextEditorScreen screen)
    {
        Editor = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public string FileFilterInput = "";
    public string FmgFilterInput = "";
    public string FmgEntryFilterInput = "";

    private bool FileFilterExactMatch = false;
    private bool FmgFilterExactMatch = false;
    private bool FmgEntryFilterExactMatch = false;

    /// <summary>
    /// Display the file filter UI
    /// </summary>
    public void DisplayFileFilterSearch()
    {
        ImGui.InputText($"Search##fileFilterSearch", ref FileFilterInput, 255);
        UIHelper.WideTooltip("Chain commands by using the + symbol between them.");

        ImGui.SameLine();
        ImGui.Checkbox($"##fileFilterExactMatch", ref FileFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFileFilterMatch(string text, string alias, TextContainerWrapper containerWrapper)
    {
        bool isValid = true;

        var input = FileFilterInput.ToLower();

        if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = text.ToLower();
            var rawAlias = alias.ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == rawText)
                    partTruth[i] = true;

                if (!FileFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!FileFilterExactMatch)
                {
                    if (rawAlias.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Only evaluate as true if all parts are true
            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }
        else if (text == null)
        {
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// Display the fmg filter UI
    /// </summary>
    public void DisplayFmgFilterSearch()
    {
        ImGui.InputText($"Search##fmgFilterSearch", ref FmgFilterInput, 255);
        UIHelper.WideTooltip("Chain commands by using the + symbol between them.");

        ImGui.SameLine();
        ImGui.Checkbox($"##fmgFilterExactMatch", ref FmgFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFmgFilterMatch(string text, string alias, int id)
    {
        bool isValid = true;

        var input = FmgFilterInput.ToLower();

        if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = text.ToLower();
            var rawAlias = alias.ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == $"{id}")
                    partTruth[i] = true;

                if (!FmgFilterExactMatch)
                {
                    if ($"{id}".Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawText)
                    partTruth[i] = true;

                if (!FmgFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!FmgFilterExactMatch)
                {
                    if (rawAlias.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Only evaluate as true if all parts are true
            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }
        else if (text == null)
        {
            isValid = false;
        }

        return isValid;
    }

    public bool FocusSelection_FmgEntryFilter = false;

    /// <summary>
    /// Display the fmg entry filter UI
    /// </summary>
    public void DisplayFmgEntryFilterSearch()
    {
        ImGui.InputText($"Search##fmgEntryFilterSearch", ref FmgEntryFilterInput, 255);
        UIHelper.WideTooltip("Chain commands by using the + symbol between them.\n\nSpecial commands:\nmodified - Displays rows where the text is different to vanilla.\nunique - Displays rows that are unique to your project.");

        if (ImGui.IsItemDeactivated())
        {
            Editor.Selection.FocusFmgEntrySelection = true;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##fmgEntryFilterExactMatch", ref FmgEntryFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFmgEntryFilterMatch(FMG.Entry curEntry)
    {
        bool isValid = true;

        var id = curEntry.ID;
        var text = curEntry.Text;

        var input = FmgEntryFilterInput.ToLower();

        if (input == "modified")
        {
            if(Editor.DifferenceManager.IsDifferentToVanilla(curEntry))
            {
                return true;
            }

            return false;
        }
        else if (input == "unique")
        {
            if (Editor.DifferenceManager.IsUniqueToProject(curEntry))
            {
                return true;
            }

            return false;
        }
        else if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = text.ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == $"{id}")
                    partTruth[i] = true;

                if (!FmgEntryFilterExactMatch)
                {
                    if ($"{id}".Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawText)
                    partTruth[i] = true;

                if (!FmgEntryFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Only evaluate as true if all parts are true
            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }
        else if(!CFG.Current.TextEditor_DisplayNullEntries && input == "" && text == null)
        {
            isValid = false;
        }
        else if (CFG.Current.TextEditor_DisplayNullEntries && input != "" && text == null)
        {
            isValid = false;
        }

        return isValid;
    }
}