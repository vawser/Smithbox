using Hexa.NET.ImGui;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor;

public class ParamListCategories
{
    public ParamEditorView View;
    public ProjectEntry Project;

    private bool isNewEntryMode = false;
    private bool isEditEntryMode = false;
    private bool isInitialEditMode = false;

    private ParamCategoryEntry _selectedUserCategory = null;

    private string NewEntryName = "";
    private bool ForceTop = false;
    private bool ForceBottom = false;
    private List<string> NewEntryParams = new List<string>();
    private int NewEntryParamsCount = 1;

    public ParamListCategories(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_ListCategories_Header")}##paramListCategoriesHeader"))
        {
            ImGui.BeginChild("ParamListCategories", ImGuiChildFlags.Borders);

            var categories = Project.Handler.ParamData.ParamCategories;

            if (categories == null)
            {
                GUI.WrappedText("No param categories found.");

                return;
            }

            var windowWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();

            GUI.WrappedText("Create or modify project-specific param categories.");
            GUI.WrappedText("");

            GUI.SimpleHeader("Actions", "");

            GUI.MultiButtonInput("baseActions",
                "saveEntries", "Save Changes", "Save the current changes made to categories.", SaveEntriesAction,
                "restoreEntries", "Restore Base Categories", "Restore the base category list.", RestoreEntriesAction);

            ImGui.Text("");

            GUI.SimpleHeader("List", "");

            ImGui.BeginChild("ParamListCategorySelectionList", new Vector2(0, 250), ImGuiChildFlags.Borders);
            foreach (var category in Project.Handler.ParamData.ParamCategories.Categories)
            {
                if (ImGui.Selectable($"{category.GetDisplayName()}##userCategory_{category.GetDisplayName()}", category == _selectedUserCategory, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedUserCategory = category;
                    isNewEntryMode = false;
                    isEditEntryMode = false;
                }
            }
            ImGui.EndChild();

            GUI.MultiButtonInput("entryActions",
                "newEntry", "Create New Entry", "Create a new category.", NewEntryAction,
                "editEntry", "Edit Selected Entry", "Edit an existing category.", EditEntryAction,
                "deleteEntry", "Delete Selected Entry", "Delete an existing category.", DeleteEntryAction);

            ImGui.Text("");

            // New Entry
            if (isNewEntryMode)
            {
                DisplayNewEntrySection();
            }

            // Edit Entry
            if (isEditEntryMode)
            {
                DisplayEditEntrySection();
            }

            ImGui.EndChild();
        }
    }

    public void NewEntryAction()
    {
        isNewEntryMode = true;
        isEditEntryMode = false;

        NewEntryName = "";
        NewEntryParamsCount = 1;
        NewEntryParams = new List<string>() { "" };
    }

    public void SaveEntriesAction()
    {
        Write();
        isNewEntryMode = false;
        isEditEntryMode = false;
    }

    public void EditEntryAction()
    {
        isNewEntryMode = false;
        isEditEntryMode = true;
        isInitialEditMode = true;
    }
    public void DeleteEntryAction()
    {
        Project.Handler.ParamData.ParamCategories.Categories.Remove(_selectedUserCategory);

        _selectedUserCategory = null;
        isNewEntryMode = false;
        isEditEntryMode = false;
    }

    public void RestoreEntriesAction()
    {
        RestoreDefault();
        isNewEntryMode = false;
        isEditEntryMode = false;
    }

    public void DisplayNewEntrySection()
    {
        GUI.SimpleHeader("New Entry Name", "");

        GUI.SinglelineTextInput("newEntryName", ref NewEntryName);
        GUI.Tooltip("The name of the new param category.");

        GUI.MultiButtonInput("newEntryActions",
            "finalizeNewEntry", "Finalize", "", FinalizeNewEntry);

        GUI.WrappedText("");
        GUI.SimpleHeader("New Entry Options", "");

        if (ImGui.Checkbox("Force to Top##newEntryforceTop", ref ForceTop))
        {
            ForceBottom = false;
        }
        GUI.Tooltip("If toggled on, this param category will always appear at the top (in alphabetically order with any other categories with the same toggle).");

        if (ImGui.Checkbox("Force to Bottom##newEntryforceBottom", ref ForceBottom))
        {
            ForceTop = false;
        }
        GUI.Tooltip("If toggled on, this param category will always appear at the bottom (in alphabetically order with any other categories with the same toggle).");

        GUI.WrappedText("");
        GUI.SimpleHeader("New Entry Params", "");

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_ParamListCategory"))
        {
            NewEntryParams.Add("");
        }
        GUI.Tooltip("Add new param target input row.");

        ImGui.SameLine();

        // Remove
        if (NewEntryParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory"))
            {
                NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
            }
            GUI.Tooltip("Remove last added param target input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory"))
            {
                NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
                GUI.Tooltip("Remove last added param target input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##paramTargetReset_ParamListCategory"))
        {
            NewEntryParams = new List<string>();
        }
        GUI.Tooltip("Reset param target input rows.");

        for (int i = 0; i < NewEntryParams.Count; i++)
        {
            var curText = NewEntryParams[i];

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
            if (ImGui.InputText($"##newParamName{i}", ref curText, 255))
            {
                NewEntryParams[i] = curText;
            }
            GUI.Tooltip("The param to include within this category.");
        }
    }

    public void FinalizeNewEntry()
    {
        isNewEntryMode = false;

        var nameEntry = new ParamCategoryNameEntry();
        nameEntry.Language = CFG.Current.ParamEditor_Annotation_Language;
        nameEntry.Name = NewEntryName;

        var newCategoryEntry = new ParamCategoryEntry();
        newCategoryEntry.Key = NewEntryName;
        newCategoryEntry.DisplayNames = [nameEntry];
        newCategoryEntry.Params = NewEntryParams;

        Project.Handler.ParamData.ParamCategories.Categories.Add(newCategoryEntry);
    }

    public void DisplayEditEntrySection()
    {
        if (_selectedUserCategory != null)
        {
            // Fill with existing stuff
            if (isInitialEditMode)
            {
                isInitialEditMode = false;

                NewEntryName = _selectedUserCategory.GetDisplayName();
                NewEntryParamsCount = _selectedUserCategory.Params.Count;
                ForceTop = _selectedUserCategory.ForceTop;
                ForceBottom = _selectedUserCategory.ForceBottom;
                NewEntryParams = _selectedUserCategory.Params;
            }

            GUI.SimpleHeader("Edit Entry", "");

            GUI.MultiButtonInput("editEntryActions",
                "finalizeEditEntry", "Finalize", "", FinalizeEditEntry);

            GUI.WrappedText("");
            GUI.SimpleHeader("Edit Entry Options", "");

            // Edit
            if (ImGui.Checkbox("Force to Top##newEntryforceTop", ref ForceTop))
            {
                ForceBottom = false;
            }
            GUI.Tooltip("If toggled on, this param category will always appear at the top (in alphabetically order with any other categories with the same toggle).");

            if (ImGui.Checkbox("Force to Bottom##newEntryforceBottom", ref ForceBottom))
            {
                ForceTop = false;
            }
            GUI.Tooltip("If toggled on, this param category will always appear at the bottom (in alphabetically order with any other categories with the same toggle).");

            GUI.WrappedText("");
            GUI.SimpleHeader("Edit Entry Params", "");

            // Add
            if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_ParamListCategory_edit"))
            {
                NewEntryParams.Add("");
            }
            GUI.Tooltip("Add new param target input row.");

            ImGui.SameLine();

            // Remove
            if (NewEntryParams.Count < 2)
            {
                ImGui.BeginDisabled();

                if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory_edit"))
                {
                    NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
                }
                GUI.Tooltip("Remove last added param target input row.");

                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory_edit"))
                {
                    NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
                    GUI.Tooltip("Remove last added param target input row.");
                }
            }

            ImGui.SameLine();

            // Reset
            if (ImGui.Button("Reset##paramTargetReset_ParamListCategory_edit"))
            {
                NewEntryParams = new List<string>();
            }
            GUI.Tooltip("Reset param target input rows.");

            for (int i = 0; i < NewEntryParams.Count; i++)
            {
                var curText = NewEntryParams[i];

                ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
                if (ImGui.InputText($"##newParamName_edit{i}", ref curText, 255))
                {
                    NewEntryParams[i] = curText;
                }
                GUI.Tooltip("The param to include within this category.");
            }
        }
    }
    public void FinalizeEditEntry()
    {
        isEditEntryMode = false;

        var curEntry = Project.Handler.ParamData.ParamCategories.Categories.Where(e => e.GetDisplayName() == NewEntryName).FirstOrDefault();

        if (curEntry != null)
        {
            curEntry.Params = NewEntryParams;
            curEntry.ForceTop = ForceTop;
            curEntry.ForceBottom = ForceBottom;
        }
    }

    public void RestoreDefault()
    {
        var projectFolder = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Categories");

        if (Directory.Exists(projectFolder))
        {
            foreach (var file in Directory.EnumerateFiles(projectFolder))
            {
                File.Delete(file);
            }
        }

        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType), "Param Categories");

        if (Directory.Exists(sourceFolder))
        {
            foreach (var file in Directory.EnumerateFiles(projectFolder))
            {
                var filename = Path.GetFileName(file);
                var projPath = Path.Combine(projectFolder, filename);

                File.Copy(file, projPath);
            }
        }
    }

    public void Write()
    {
        var modResourceDir = Path.Join(Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(Project));
        var modResourcePath = Path.Combine(modResourceDir, "Param Categories.json");

        if (!Directory.Exists(modResourceDir))
        {
            Directory.CreateDirectory(modResourceDir);
        }

        try
        {
            string jsonString = JsonSerializer.Serialize(Project.Handler.ParamData.ParamCategories, typeof(ParamCategories), ParamEditorJsonSerializerContext.Default);
            var fs = new FileStream(modResourcePath, System.IO.FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, "Failed to write project param categories", ex);
        }

    }
}
