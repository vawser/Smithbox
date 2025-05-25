using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Core;
using StudioCore.Interface;

namespace StudioCore.EzStateEditorNS;

public class EsdFilters
{
    public EsdEditorScreen Editor;
    public ProjectEntry Project;

    public EsdFilters(EsdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public string FileFilterInput = "";
    public string InternalFileFilterInput = "";
    public string StateGroupFilterInput = "";
    public string StateFilterInput = "";

    private bool FileFilterExactMatch = false;
    private bool InternalFileFilterExactMatch = false;
    private bool StateGroupFilterExactMatch = false;
    private bool StateFilterExactMatch = false;

    public void DisplayFileFilterSearch()
    {
        ImGui.InputText($"Search##fileFilterSearch", ref FileFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##fileFilterExactMatch", ref FileFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

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

        return isValid;
    }

    public void DisplayScriptFilterSearch()
    {
        ImGui.InputText($"Search##internalFileFilterSearch", ref InternalFileFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##internalFileFilterExactMatch", ref InternalFileFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    public bool IsScriptFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = InternalFileFilterInput.ToLower();

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

                if (!InternalFileFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!InternalFileFilterExactMatch)
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

    public void DisplayStateGroupFilterSearch()
    {
        ImGui.InputText($"Search##stateGroupFilterSearch", ref StateGroupFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##stateGroupFilterExactMatch", ref StateGroupFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    public bool IsStateGroupFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = StateGroupFilterInput.ToLower();

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

                if (!StateGroupFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!StateGroupFilterExactMatch)
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

    public void DisplayStateFilterSearch()
    {
        ImGui.InputText($"Search##stateFilterSearch", ref StateFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##stateFilterExactMatch", ref StateFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    public bool IsStateFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = StateFilterInput.ToLower();

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

                if (!StateFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!StateFilterExactMatch)
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
}

