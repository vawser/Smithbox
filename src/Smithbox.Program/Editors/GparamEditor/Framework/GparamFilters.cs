using Hexa.NET.ImGui;
using StudioCore.Interface;

namespace StudioCore.GraphicsParamEditorNS;

public class GparamFilters
{
    private GparamEditorScreen Screen;

    public string FileFilterInput = "";
    public string GroupFilterInput = "";
    public string FieldFilterInput = "";
    public string FieldValueFilterInput = "";

    private bool FileFilterExactMatch = false;
    private bool GroupFilterExactMatch = false;
    private bool FieldFilterExactMatch = false;
    private bool FieldValueFilterExactMatch = false;

    public GparamFilters(GparamEditorScreen screen)
    {
        Screen = screen;
    }

    /// <summary>
    /// Display the file filter UI
    /// </summary>
    public void DisplayFileFilterSearch()
    {
        ImGui.InputText($"Search##fileFilterSearch", ref FileFilterInput, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.SameLine();
        ImGui.Checkbox($"##fileFilterExactMatch", ref FileFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
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

        return isValid;
    }

    /// <summary>
    /// Display the group filter UI
    /// </summary>
    public void DisplayGroupFilterSearch()
    {
        ImGui.InputText($"Search##groupFilterSearch", ref GroupFilterInput, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.SameLine();
        ImGui.Checkbox($"##groupFilterExactMatch", ref GroupFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsGroupFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = GroupFilterInput.ToLower();

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

                if (!GroupFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!GroupFilterExactMatch)
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

    /// <summary>
    /// Display the field filter UI
    /// </summary>
    public void DisplayFieldFilterSearch()
    {
        ImGui.InputText($"Search##fieldFilterSearch", ref FieldFilterInput, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.SameLine();
        ImGui.Checkbox($"##fieldFilterExactMatch", ref FieldFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFieldFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = FieldFilterInput.ToLower();

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

                if (!FieldFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!FieldFilterExactMatch)
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

    /// <summary>
    /// Display the field value filter UI
    /// </summary>
    public void DisplayFieldValueFilterSearch()
    {
        ImGui.InputText($"Search##fieldValueFilterSearch", ref FieldValueFilterInput, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");

        ImGui.SameLine();
        ImGui.Checkbox($"##fieldValueFilterExactMatch", ref FieldValueFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFieldValueFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = FieldValueFilterInput.ToLower();

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

                if (!FieldFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!FieldFilterExactMatch)
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
