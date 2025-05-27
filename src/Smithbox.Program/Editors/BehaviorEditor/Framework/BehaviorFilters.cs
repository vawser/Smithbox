using BehaviorEditorNS;
using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorFilters
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorFilters(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    // Binder List
    public string BinderListInput = "";
    public bool BinderListInput_IgnoreCase = true;

    // Data Categories
    public string DataCategoriesInput = "";
    public bool DataCategoriesInput_IgnoreCase = true;

    // Data Entries
    public string DataEntriesInput = "";
    public bool DataEntriesInput_IgnoreCase = true;

    // Field List
    public string FieldListInput = "";
    public bool FieldListInput_IgnoreCase = true;

    // Basic matcher for now
    public bool IsBasicMatch(ref string inputText, bool ignoreCase, string entryText, string aliasText)
    {
        bool isValid = true;

        // Verify the regex pattern is valid, if not clear it
        if(!IsValidRegex(inputText))
        {
            inputText = "";
            return true;
        }

        var input = inputText.ToLower();

        if (input != "" && entryText != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = entryText.ToLower();
            var rawAlias = aliasText.ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (ignoreCase)
                {
                    var match = Regex.Match(rawText, entry, RegexOptions.IgnoreCase);

                    if (match.Success)
                        partTruth[i] = true;
                }
                else
                {
                    var match = Regex.Match(rawText, entry);

                    if (match.Success)
                        partTruth[i] = true;
                }

                if (rawAlias != "")
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

        return isValid;
    }

    public bool IsValidRegex(string pattern)
    {
        try
        {
            _ = new Regex(pattern);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
