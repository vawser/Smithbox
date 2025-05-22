using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.TextureViewer;

namespace StudioCore.Editors.TextureViewer;

public class TexFilters
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexFilters(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public string FileFilterSearchStr = "";
    public string TpfFilterSearchStr = "";
    public string TextureFilterSearchStr = "";

    /// <summary>
    /// Display the texture file filter UI
    /// </summary>
    public void DisplayFileFilterSearch()
    {
        ImGui.InputText($"Search##textureFileFilterSearch", ref FileFilterSearchStr, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFileFilterMatch(string text, string alias)
    {
        bool match = false;

        string cleanRawInput = FileFilterSearchStr.Trim().ToLower();
        string cleanCheckInput = text.Trim().ToLower();
        string clearAliasInput = alias.Trim().ToLower();

        if (cleanRawInput.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = cleanRawInput.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            if (entry == cleanCheckInput)
                partTruth[i] = true;

            if (cleanCheckInput.Contains(entry))
                partTruth[i] = true;

            if (entry == clearAliasInput)
                partTruth[i] = true;

            if (clearAliasInput.Contains(entry))
                partTruth[i] = true;

            var refParts = cleanCheckInput.Split($"_");
            foreach (var refPart in refParts)
            {
                if (entry == refPart)
                    partTruth[i] = true;

                if (refPart.Contains(entry))
                    partTruth[i] = true;
            }

            var refNameParts = clearAliasInput.Split($" ");
            foreach (var refNamePart in refNameParts)
            {
                if (entry == refNamePart)
                    partTruth[i] = true;

                if (refNamePart.Contains(entry))
                    partTruth[i] = true;
            }
        }

        match = true;

        foreach (bool entry in partTruth)
        {
            if (!entry)
                match = false;
        }

        return match;
    }

    /// <summary>
    /// Display the texture list filter UI
    /// </summary>
    public void DisplayTpfFilterSearch()
    {
        ImGui.InputText($"Search##textureListFilterSearch", ref TpfFilterSearchStr, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsTpfFilterMatch(string text)
    {
        bool match = false;

        string cleanRawInput = TpfFilterSearchStr.Trim().ToLower();
        string cleanCheckInput = text.Trim().ToLower();

        if (cleanRawInput.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = cleanRawInput.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            if (entry == cleanCheckInput)
                partTruth[i] = true;

            if (cleanCheckInput.Contains(entry))
                partTruth[i] = true;

            var refParts = cleanCheckInput.Split($"_");
            foreach (var refPart in refParts)
            {
                if (entry == refPart)
                    partTruth[i] = true;

                if (refPart.Contains(entry))
                    partTruth[i] = true;
            }
        }

        match = true;

        foreach (bool entry in partTruth)
        {
            if (!entry)
                match = false;
        }

        return match;
    }

    /// <summary>
    /// Display the texture list filter UI
    /// </summary>
    public void DisplayTextureFilterSearch()
    {
        ImGui.InputText($"Search##textureListFilterSearch", ref TextureFilterSearchStr, 255);
        UIHelper.Tooltip("Separate terms are split via the + character.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsTextureFilterMatch(string text)
    {
        bool match = false;

        string cleanRawInput = TextureFilterSearchStr.Trim().ToLower();
        string cleanCheckInput = text.Trim().ToLower();

        if (cleanRawInput.Equals(""))
        {
            match = true; // If input is empty, show all
            return match;
        }

        string[] inputParts = cleanRawInput.Split("+");
        bool[] partTruth = new bool[inputParts.Length];

        for (int i = 0; i < partTruth.Length; i++)
        {
            string entry = inputParts[i];

            if (entry == cleanCheckInput)
                partTruth[i] = true;

            if (cleanCheckInput.Contains(entry))
                partTruth[i] = true;

            var refParts = cleanCheckInput.Split($"_");
            foreach (var refPart in refParts)
            {
                if (entry == refPart)
                    partTruth[i] = true;

                if (refPart.Contains(entry))
                    partTruth[i] = true;
            }
        }

        match = true;

        foreach (bool entry in partTruth)
        {
            if (!entry)
                match = false;
        }

        return match;
    }
}
