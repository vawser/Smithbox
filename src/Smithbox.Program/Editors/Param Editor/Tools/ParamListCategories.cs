using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamListCategories
{
    public ParamEditorScreen Editor;
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

    public ParamListCategories(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Param List Categories"))
        {
            var categories = Project.Handler.ParamData.ParamCategories;

            if (categories == null)
            {
                UIHelper.WrappedText("No param categories found.");

                return;
            }

            var windowWidth = ImGui.GetWindowWidth();
            var sectionHeight = ImGui.GetWindowHeight();

            UIHelper.WrappedText("Create or modify project-specific param categories.");
            UIHelper.WrappedText("");

            ImGui.Separator();

            if (ImGui.Button("New Entry"))
            {
                isNewEntryMode = true;
                isEditEntryMode = false;

                NewEntryName = "";
                NewEntryParamsCount = 1;
                NewEntryParams = new List<string>() { "" };
            }
            UIHelper.Tooltip("Create a new param category.");

            ImGui.SameLine();
            if (ImGui.Button("Save Changes"))
            {
                Write();
                isNewEntryMode = false;
                isEditEntryMode = false;
            }
            UIHelper.Tooltip("Permanently save the current param categories to your project's .smithbox folder, so they persist across sessions.");

            if (ImGui.Button("Edit Selected Entry"))
            {
                isNewEntryMode = false;
                isEditEntryMode = true;
                isInitialEditMode = true;
            }
            UIHelper.Tooltip("Edit the currently selected param category.");

            ImGui.SameLine();
            if (ImGui.Button("Delete Selected Entry"))
            {
                Project.Handler.ParamData.ParamCategories.Categories.Remove(_selectedUserCategory);

                _selectedUserCategory = null;
                isNewEntryMode = false;
                isEditEntryMode = false;
            }
            UIHelper.Tooltip("Delete the currently selected param category.");

            if (ImGui.Button("Restore Base Categories"))
            {
                RestoreDefault();
                isNewEntryMode = false;
                isEditEntryMode = false;
            }
            UIHelper.Tooltip("Restore the default param categories.");

            // List
            ImGui.Separator();

            foreach (var category in Project.Handler.ParamData.ParamCategories.Categories)
            {
                if (ImGui.Selectable($"{category.DisplayName}##userCategory_{category.DisplayName}", category == _selectedUserCategory, ImGuiSelectableFlags.AllowDoubleClick))
                {
                    _selectedUserCategory = category;
                    isNewEntryMode = false;
                    isEditEntryMode = false;
                }
            }

            ImGui.Separator();

            // New Entry
            if (isNewEntryMode)
            {
                UIHelper.WrappedText("New Param Category");
                ImGui.Separator();

                ImGui.InputText("Name##newEntryName", ref NewEntryName, 255);
                UIHelper.Tooltip("The name of this param category.");

                if (ImGui.Checkbox("Force to Top##newEntryforceTop", ref ForceTop))
                {
                    ForceBottom = false;
                }
                UIHelper.Tooltip("If toggled on, this param category will always appear at the top (in alphabetically order with any other categories with the same toggle).");

                if (ImGui.Checkbox("Force to Bottom##newEntryforceBottom", ref ForceBottom))
                {
                    ForceTop = false;
                }
                UIHelper.Tooltip("If toggled on, this param category will always appear at the bottom (in alphabetically order with any other categories with the same toggle).");

                ImGui.Text("Params to add:");
                for (int i = 0; i < NewEntryParamsCount; i++)
                {
                    var curText = NewEntryParams[i];
                    ImGui.InputText($"##newParamName{i}", ref curText, 255);
                    NewEntryParams[i] = curText;

                    ImGui.SameLine();

                    if (NewEntryParams.Count > 1)
                    {
                        if (ImGui.Button($"Remove##removeNewParamName{i}"))
                        {
                            NewEntryParams.RemoveAt(i);
                            NewEntryParamsCount = NewEntryParams.Count;
                        }
                    }
                }

                ImGui.Text("");

                if (ImGui.Button("Expand List"))
                {
                    NewEntryParams.Add("");
                    NewEntryParamsCount++;
                }
                UIHelper.Tooltip("Add another param entry to fill.");

                ImGui.SameLine();

                if (ImGui.Button("Finalize Entry"))
                {
                    isNewEntryMode = false;

                    var newCategoryEntry = new ParamCategoryEntry();
                    newCategoryEntry.DisplayName = NewEntryName;
                    newCategoryEntry.Params = NewEntryParams;

                    Project.Handler.ParamData.ParamCategories.Categories.Add(newCategoryEntry);
                }
            }

            // Edit Entry
            if (isEditEntryMode)
            {
                UIHelper.WrappedText("Edit Param Category");
                ImGui.Separator();

                if (_selectedUserCategory != null)
                {
                    // Fill with existing stuff
                    if (isInitialEditMode)
                    {
                        isInitialEditMode = false;

                        NewEntryName = _selectedUserCategory.DisplayName;
                        NewEntryParamsCount = _selectedUserCategory.Params.Count;
                        ForceTop = _selectedUserCategory.ForceTop;
                        ForceBottom = _selectedUserCategory.ForceBottom;
                        NewEntryParams = _selectedUserCategory.Params;
                    }

                    // Edit
                    if (ImGui.Checkbox("Force to Top##newEntryforceTop", ref ForceTop))
                    {
                        ForceBottom = false;
                    }
                    UIHelper.Tooltip("If toggled on, this param category will always appear at the top (in alphabetically order with any other categories with the same toggle).");

                    if (ImGui.Checkbox("Force to Bottom##newEntryforceBottom", ref ForceBottom))
                    {
                        ForceTop = false;
                    }
                    UIHelper.Tooltip("If toggled on, this param category will always appear at the bottom (in alphabetically order with any other categories with the same toggle).");

                    if (ImGui.Button("Expand List"))
                    {
                        NewEntryParams.Add("");
                        NewEntryParamsCount++;
                    }
                    UIHelper.Tooltip("Add another param entry to fill.");

                    ImGui.SameLine();

                    if (ImGui.Button("Finalize Entry"))
                    {
                        isEditEntryMode = false;

                        var curEntry = Project.Handler.ParamData.ParamCategories.Categories.Where(e => e.DisplayName == NewEntryName).FirstOrDefault();

                        if (curEntry != null)
                        {
                            curEntry.Params = NewEntryParams;
                            curEntry.ForceTop = ForceTop;
                            curEntry.ForceBottom = ForceBottom;
                        }
                    }

                    ImGui.Text("Params to add:");
                    for (int i = 0; i < NewEntryParamsCount; i++)
                    {
                        var curText = NewEntryParams[i];
                        ImGui.InputText($"##newParamName{i}", ref curText, 255);
                        NewEntryParams[i] = curText;

                        ImGui.SameLine();

                        if (NewEntryParams.Count > 1)
                        {
                            if (ImGui.Button($"Remove##removeNewParamName{i}"))
                            {
                                NewEntryParams.RemoveAt(i);
                                NewEntryParamsCount = NewEntryParams.Count;
                            }
                        }
                    }
                }
            }
        }
    }

    public void RestoreDefault()
    {
        var sourceFolder = Path.Join(StudioCore.Common.FileLocations.Assets, "PARAM", ProjectUtils.GetGameDirectory(Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Param Categories.json");

        if (File.Exists(sourceFile))
        {
            try
            {
                var filestring = File.ReadAllText(sourceFile);

                try
                {
                    Editor.Project.Handler.ParamData.ParamCategories = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamCategoryResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddError("Failed to deserialize param categories", e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddError("Failed to read param categories", e);
            }
        }
        else
        {
            TaskLogs.AddError("Failed to find default param categories for game");
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
            string jsonString = JsonSerializer.Serialize(Project.Handler.ParamData.ParamCategories, typeof(ParamCategoryResource), ParamEditorJsonSerializerContext.Default);
            var fs = new FileStream(modResourcePath, System.IO.FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            TaskLogs.AddError("Failed to write project param categories", ex);
        }

    }
}
