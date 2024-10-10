
using ImGuiNET;
using Microsoft.AspNetCore.Components.Forms;
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
    private TextEditorScreen Screen;
    private TextPropertyDecorator Decorator;
    private TextSelectionManager Selection;

    public TextFilters(TextEditorScreen screen)
    {
        Screen = screen;
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

        ImGui.SameLine();
        ImGui.Checkbox($"##fileFilterExactMatch", ref FileFilterExactMatch);
        UIHelper.ShowHoverTooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFileFilterMatch(string text, string alias)
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

        ImGui.SameLine();
        ImGui.Checkbox($"##fmgFilterExactMatch", ref FmgFilterExactMatch);
        UIHelper.ShowHoverTooltip("Filter will ignore partial matches when enabled.");
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

    /// <summary>
    /// Display the fmg entry filter UI
    /// </summary>
    public void DisplayFmgEntryFilterSearch()
    {
        ImGui.InputText($"Search##fmgEntryFilterSearch", ref FmgEntryFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##fmgEntryFilterExactMatch", ref FmgEntryFilterExactMatch);
        UIHelper.ShowHoverTooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFmgEntryFilterMatch(string text, int id)
    {
        bool isValid = true;

        var input = FmgEntryFilterInput.ToLower();

        if (input != "" && text != null)
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