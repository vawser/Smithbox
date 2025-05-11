using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.MaterialEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

public class MaterialFilters
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialFilters(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public string BinderFilterInput = "";
    public string FileFilterInput = "";

    public bool BinderFilterExactMatch = false;
    public bool FileFilterExactMatch = false;

    public void DisplayBinderFilterSearch()
    {
        ImGui.InputText($"Search##binderFilterSearch", ref BinderFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##binderFilterExactMatch", ref BinderFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    public bool IsBinderFilterMatch(string text)
    {
        bool isValid = true;

        var input = BinderFilterInput.ToLower();

        if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = text.ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == rawText)
                    partTruth[i] = true;

                if (!BinderFilterExactMatch)
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

        return isValid;
    }

    public void DisplayFileFilterSearch()
    {
        ImGui.InputText($"Search##fileFilterSearch", ref FileFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##fileFilterExactMatch", ref FileFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    public bool IsFileFilterMatch(string text)
    {
        bool isValid = true;

        var input = FileFilterInput.ToLower();

        if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = text.ToLower();

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
}
