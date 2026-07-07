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
        if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_ListCat_Header")}##paramListCategoriesHeader"))
        {
            ImGui.BeginChild("ParamListCategories", ImGuiChildFlags.Borders);

            var categories = Project.Handler.ParamData.ParamCategories;

            if (categories == null)
            {
                GUI.WrappedText(LOC.Get("PARAM_ListCat_No_Categories"));

                return;
            }

            var windowWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();

            GUI.WrappedText(LOC.Get("PARAM_ListCat_Hint"));

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ListCat_Header_Actions"),
                LOC.Get("PARAM_ListCat_Header_Actions_TT"));

            GUI.MultiButtonInput("baseActions",
                "saveEntries", 
                LOC.Get("PARAM_ListCat_Action_Save"),
                LOC.Get("PARAM_ListCat_Action_Save_TT"),
                SaveEntriesAction,

                "restoreEntries", 
                LOC.Get("PARAM_ListCat_Action_Restore"),
                LOC.Get("PARAM_ListCat_Action_Restore_TT"),
                RestoreEntriesAction);

            // List
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ListCat_Header_List"),
                LOC.Get("PARAM_ListCat_Header_List_TT"));

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
                "newEntry", 
                LOC.Get("PARAM_ListCat_Action_Create_Entry"),
                LOC.Get("PARAM_ListCat_Action_Create_Entry_TT"),
                NewEntryAction,

                "editEntry", 
                LOC.Get("PARAM_ListCat_Action_Edit_Entry"),
                LOC.Get("PARAM_ListCat_Action_Edit_Entry_TT"),
                EditEntryAction,

                "deleteEntry", 
                LOC.Get("PARAM_ListCat_Action_Delete_Entry"),
                LOC.Get("PARAM_ListCat_Action_Delete_Entry_TT"),
                DeleteEntryAction);

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
        GUI.SimpleHeader(
            LOC.Get("PARAM_ListCat_NewEntry_Header"),
            LOC.Get("PARAM_ListCat_NewEntry_Header_TT"));

        GUI.SinglelineTextInput("newEntryName", ref NewEntryName);
        GUI.Tooltip(LOC.Get("PARAM_ListCat_NewEntry_TT"));

        GUI.MultiButtonInput("newEntryActions",
            "finalizeNewEntry", 
            LOC.Get("PARAM_ListCat_NewEntry_Action_Finalize"),
            LOC.Get("PARAM_ListCat_NewEntry_Action_Finalize_TT"),
            FinalizeNewEntry);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ListCat_Entry_Options"),
            LOC.Get("PARAM_ListCat_Entry_Options_TT"));

        if (ImGui.Checkbox($"{LOC.Get("PARAM_ListCat_Checkbox_Force_Top")}##newEntryforceTop", ref ForceTop))
        {
            ForceBottom = false;
        }
        GUI.Tooltip(LOC.Get("PARAM_ListCat_Checkbox_Force_Top_TT"));

        if (ImGui.Checkbox($"{LOC.Get("PARAM_ListCat_Checkbox_Force_Bottom")}##newEntryforceBottom", ref ForceBottom))
        {
            ForceTop = false;
        }
        GUI.Tooltip(LOC.Get("PARAM_ListCat_Checkbox_Force_Bottom_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_ListCat_Entry_Header_Parameters"),
            LOC.Get("PARAM_ListCat_Entry_Header_Parameters_TT"));

        // Add
        if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_ParamListCategory"))
        {
            NewEntryParams.Add("");
        }
        GUI.Tooltip(LOC.Get("PARAM_ListCat_Add_Param_Target_TT"));

        ImGui.SameLine();

        // Remove
        if (NewEntryParams.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory"))
            {
                NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Remove_Param_Target_TT"));

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory"))
            {
                NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Remove_Param_Target_TT"));
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button($"{LOC.Get("PARAM_ListCat_Reset_Param_Target")}##paramTargetReset_ParamListCategory"))
        {
            NewEntryParams = new List<string>();
        }
        GUI.Tooltip(LOC.Get("PARAM_ListCat_Reset_Param_Target_TT"));

        for (int i = 0; i < NewEntryParams.Count; i++)
        {
            var curText = NewEntryParams[i];

            ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
            if (ImGui.InputText($"##newParamName{i}", ref curText, 255))
            {
                NewEntryParams[i] = curText;
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Param_Target_Include_TT"));
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

            GUI.SimpleHeader(
                LOC.Get("PARAM_ListCat_EditEntry_Header"),
                LOC.Get("PARAM_ListCat_EditEntry_Header_TT"));

            GUI.MultiButtonInput("editEntryActions",
                "finalizeEditEntry", 
                LOC.Get("PARAM_ListCat_EditEntry_Action_Finalize"),
                LOC.Get("PARAM_ListCat_EditEntry_Action_Finalize_TT"),
                FinalizeEditEntry);

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ListCat_Entry_Options"),
                LOC.Get("PARAM_ListCat_Entry_Options_TT"));

            // Edit
            if (ImGui.Checkbox($"{LOC.Get("PARAM_ListCat_Checkbox_Force_Top")}##newEntryforceTop", ref ForceTop))
            {
                ForceBottom = false;
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Checkbox_Force_Top_TT"));

            if (ImGui.Checkbox($"{LOC.Get("PARAM_ListCat_Checkbox_Force_Bottom")}##newEntryforceBottom", ref ForceBottom))
            {
                ForceTop = false;
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Checkbox_Force_Bottom_TT"));

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_ListCat_Entry_Header_Parameters"),
                LOC.Get("PARAM_ListCat_Entry_Header_Parameters_TT"));

            // Add
            if (ImGui.Button($"{Icons.Plus}##paramTargetAdd_ParamListCategory_edit"))
            {
                NewEntryParams.Add("");
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Add_Param_Target_TT"));

            ImGui.SameLine();

            // Remove
            if (NewEntryParams.Count < 2)
            {
                ImGui.BeginDisabled();

                if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory_edit"))
                {
                    NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
                }
                GUI.Tooltip(LOC.Get("PARAM_ListCat_Remove_Param_Target_TT"));

                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"{Icons.Minus}##paramTargetRemove_ParamListCategory_edit"))
                {
                    NewEntryParams.RemoveAt(NewEntryParams.Count - 1);
                }
                GUI.Tooltip(LOC.Get("PARAM_ListCat_Remove_Param_Target_TT"));
            }

            ImGui.SameLine();

            // Reset
            if (ImGui.Button($"{LOC.Get("PARAM_ListCat_Reset_Param_Target")}##paramTargetReset_ParamListCategory_edit"))
            {
                NewEntryParams = new List<string>();
            }
            GUI.Tooltip(LOC.Get("PARAM_ListCat_Reset_Param_Target_TT"));

            for (int i = 0; i < NewEntryParams.Count; i++)
            {
                var curText = NewEntryParams[i];

                ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
                if (ImGui.InputText($"##newParamName_edit{i}", ref curText, 255))
                {
                    NewEntryParams[i] = curText;
                }
                GUI.Tooltip(LOC.Get("PARAM_ListCat_Param_Target_Include_TT"));
            }
        }
    }
    public void FinalizeEditEntry()
    {
        isEditEntryMode = false;

        var curEntry = Project.Handler.ParamData.ParamCategories.Categories.FirstOrDefault(e => e.GetDisplayName() == NewEntryName);

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
            Smithbox.LogError(this, LOC.Get("PARAM_ListCat_Failed_Write"), ex);
        }

    }
}
