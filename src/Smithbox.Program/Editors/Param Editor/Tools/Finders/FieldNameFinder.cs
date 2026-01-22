using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class FieldNameFinder
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public string imguiID = "FieldNameFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public bool IncludeCommunityNameInSearch = true;
    public bool IncludeDescriptionInSearch = true;
    public bool MatchTextExactly = false;
    public bool DisplayCommunityNameInResult = false;

    public List<string> TargetedParams = new();

    public List<DataSearchResult> Results = new();

    public FieldNameFinder(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (Editor.Project.Handler.ParamData.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();

        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display all fields and the respective params they appear in based on the search text.");
        UIHelper.WrappedText("");

        /// Targeted Param
        UIHelper.SimpleHeader("Targeted Params", "Leave blank to target all params.");

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_fieldIdFinder"))
        {
            TargetedParams.Add("");
        }
        UIHelper.Tooltip("Add new param target input row.");

        ImGui.SameLine();

        // Remove
        if (TargetedParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_fieldIdFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
            }
            UIHelper.Tooltip("Remove last added param target input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_fieldIdFinder"))
            {
                TargetedParams.RemoveAt(TargetedParams.Count - 1);
                UIHelper.Tooltip("Remove last added param target input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##paramTargetReset_fieldIdFinder"))
        {
            TargetedParams = new List<string>();
        }
        UIHelper.Tooltip("Reset param target input rows.");

        for (int i = 0; i < TargetedParams.Count; i++)
        {
            var curCommand = TargetedParams[i];
            var curText = curCommand;

            if (ImGui.InputText($"##paramTargetInput{i}_fieldIdFinder", ref curText, 255))
            {
                TargetedParams[i] = curText;
            }
            UIHelper.Tooltip("The param target to include.");
        }

        UIHelper.WrappedText("");

        /// Search Configuration
        UIHelper.SimpleHeader("Search Configuration", "The configuration parameters for the search.");

        // Checkbox: Include Community Name in Search
        ImGui.Checkbox($"Include Community Name in Search##includeCommunityName_{imguiID}",
            ref IncludeCommunityNameInSearch);

        UIHelper.Tooltip("Include the community name text for a field in the search.");

        // Checkbox: Include Descriptions in Search
        ImGui.Checkbox($"Include Descriptions in Search##includeDescriptions_{imguiID}",
            ref IncludeDescriptionInSearch);

        UIHelper.Tooltip("Include the description text for a field in the search.");

        // Checkbox: Match Exactly
        ImGui.Checkbox($"Complete Word Match##matchExact_{imguiID}",
            ref MatchTextExactly);

        UIHelper.Tooltip("When matching, ensure the search term is an exact match for a word, not a partial element of the word." +
            "\nFor internal names, this will split the string based on capitalization before checking.");

        // Checkbox: Display Community Name in Results
        ImGui.Checkbox($"Display Community Name in Results##useCommunityNames_{imguiID}",
            ref DisplayCommunityNameInResult);
        UIHelper.Tooltip("Display the community name for the field instead of the internal name.");

        UIHelper.WrappedText("");

        // Search Text
        UIHelper.WrappedText("Search Text:");

        ImGui.InputText("##searchString", ref SearchText, 255);
        UIHelper.Tooltip("The text to search for. Matches loosely by default.");

        // Search Button
        if (ImGui.Button($"Search##searchButton_{imguiID}"))
        {
            if (Editor.Project.Handler.ParamData.PrimaryBank.Params != null)
            {
                CachedSearchText = SearchText;

                Results = ConstructResults();
                Results.Sort();
            }
        }

        UIHelper.WrappedText("");

        // Results List
        if (Results.Count > 0)
        {
            UIHelper.SimpleHeader("Search Results", "The results of the last search performed.");

            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param: Row Name");

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(EditX, EditY));

            foreach (var result in Results)
            {
                var name = result.FieldInternalName;

                if (DisplayCommunityNameInResult)
                {
                    name = result.FieldDisplayName;
                }

                if (ImGui.Selectable($"{result.ParamName}: {name}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}");
                }
            }

            ImGui.EndChild();
        }
        else
        {
            ImGui.Text("No results to display.");
        }

        UIHelper.WrappedText("");
    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchComponents = SearchText.ToLower().Split(" ");

        foreach (var p in Editor.Project.Handler.ParamData.PrimaryBank.Params)
        {
            if (TargetedParams.Count > 0)
            {
                if (!TargetedParams.Contains(p.Key))
                {
                    continue;
                }
            }

            var def = p.Value.AppliedParamdef;
            var meta = Editor.Project.Handler.ParamData.GetParamMeta(def);

            foreach (var field in def.Fields)
            {
                bool addResult = false;
                var fieldMeta = Editor.Project.Handler.ParamData.GetParamFieldMeta(meta, field);

                foreach (var entry in searchComponents)
                {
                    // Internal Name
                    if (field.InternalName != null)
                    {
                        if (MatchTextExactly)
                        {
                            var adjustedName = field.InternalName;

                            // Place _ in front of inter-word capitalization
                            adjustedName = Regex.Replace(adjustedName, "(?<!^)([A-Z])", "_$1");

                            var adjustedComponents = adjustedName.Split("_");

                            foreach (var component in adjustedComponents)
                            {
                                if (component.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                        }
                        else
                        {
                            if (field.InternalName.ToLower().Contains(entry))
                            {
                                addResult = true;
                            }
                        }
                    }

                    // Display Name
                    if (fieldMeta.AltName != null && IncludeCommunityNameInSearch)
                    {
                        var displayNameComponents = fieldMeta.AltName.Split(" ");

                        foreach (var displayComponent in displayNameComponents)
                        {
                            if (MatchTextExactly)
                            {
                                if (displayComponent.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (displayComponent.ToLower().Contains(entry))
                                {
                                    addResult = true;
                                }
                            }
                        }
                    }

                    // Wiki
                    if (fieldMeta.Wiki != null && IncludeDescriptionInSearch)
                    {
                        var descriptionComponents = fieldMeta.Wiki.Split(" ");

                        foreach (var descriptionComponent in descriptionComponents)
                        {
                            if (MatchTextExactly)
                            {
                                if (descriptionComponent.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (descriptionComponent.ToLower().Contains(entry))
                                {
                                    addResult = true;
                                }
                            }
                        }
                    }
                }

                if (addResult)
                {
                    var result = new DataSearchResult();
                    result.FieldInternalName = field.InternalName;
                    result.FieldDisplayName = fieldMeta.AltName;
                    result.ParamName = p.Key;

                    output.Add(result);
                }
            }
        }

        return output;
    }
}
