using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor;

public static class ParamCategories
{
    private static bool isNewEntryMode = false;
    private static bool isEditEntryMode = false;
    private static bool isInitialEditMode = false;

    private static ParamCategoryEntry _selectedUserCategory = null;

    private static string NewEntryName = "";
    private static bool ForceTop = false;
    private static bool ForceBottom = false;
    private static List<string> NewEntryParams = new List<string>();
    private static int NewEntryParamsCount = 1;

    public static void Display(ParamEditorScreen editor)
    {
        var categories = editor.Project.Handler.ParamData.ParamCategories;

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

        if (ImGui.Button("New Entry", DPI.HalfWidthButton(windowWidth, 24)))
        {
            isNewEntryMode = true;
            isEditEntryMode = false;

            NewEntryName = "";
            NewEntryParamsCount = 1;
            NewEntryParams = new List<string>() { "" };
        }
        UIHelper.Tooltip("Create a new param category.");

        ImGui.SameLine();
        if (ImGui.Button("Save Changes", DPI.HalfWidthButton(windowWidth, 24)))
        {
            Write(editor);
            isNewEntryMode = false;
            isEditEntryMode = false;
        }
        UIHelper.Tooltip("Permanently save the current param categories to your project's .smithbox folder, so they persist across sessions.");

        if (ImGui.Button("Edit Selected Entry", DPI.HalfWidthButton(windowWidth, 24)))
        {
            isNewEntryMode = false;
            isEditEntryMode = true;
            isInitialEditMode = true;
        }
        UIHelper.Tooltip("Edit the currently selected param category.");

        ImGui.SameLine();
        if (ImGui.Button("Delete Selected Entry", DPI.HalfWidthButton(windowWidth, 24)))
        {
            editor.Project.Handler.ParamData.ParamCategories.Categories.Remove(_selectedUserCategory);
            _selectedUserCategory = null;
            isNewEntryMode = false;
            isEditEntryMode = false;
        }
        UIHelper.Tooltip("Delete the currently selected param category.");

        if (ImGui.Button("Restore Base Categories", DPI.WholeWidthButton(windowWidth, 24)))
        {
            var result = PlatformUtils.Instance.MessageBox("Are you sure?", "Warning", MessageBoxButtons.YesNo);

            if (result is DialogResult.Yes)
            {
                RestoreDefault(editor);
                isNewEntryMode = false;
                isEditEntryMode = false;
            }
        }
        UIHelper.Tooltip("Restore the default param categories.");

        // List
        ImGui.Separator();

        foreach(var category in editor.Project.Handler.ParamData.ParamCategories.Categories)
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
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "New Param Category");
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
                    if (ImGui.Button($"Remove##removeNewParamName{i}", DPI.StandardButtonSize))
                    {
                        NewEntryParams.RemoveAt(i);
                        NewEntryParamsCount = NewEntryParams.Count;
                    }
                }
            }

            ImGui.Text("");

            if (ImGui.Button("Expand List", DPI.HalfWidthButton(windowWidth, 24)))
            {
                NewEntryParams.Add("");
                NewEntryParamsCount++;
            }
            UIHelper.Tooltip("Add another param entry to fill.");

            ImGui.SameLine();

            if (ImGui.Button("Finalize Entry", DPI.HalfWidthButton(windowWidth, 24)))
            {
                isNewEntryMode = false;

                var newCategoryEntry = new ParamCategoryEntry();
                newCategoryEntry.DisplayName = NewEntryName;
                newCategoryEntry.Params = NewEntryParams;

                editor.Project.Handler.ParamData.ParamCategories.Categories.Add(newCategoryEntry);
            }
        }

        // Edit Entry
        if (isEditEntryMode)
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Edit Param Category");
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

                if (ImGui.Button("Expand List", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    NewEntryParams.Add("");
                    NewEntryParamsCount++;
                }
                UIHelper.Tooltip("Add another param entry to fill.");

                ImGui.SameLine();

                if (ImGui.Button("Finalize Entry", DPI.HalfWidthButton(windowWidth, 24)))
                {
                    isEditEntryMode = false;

                    var curEntry = editor.Project.Handler.ParamData.ParamCategories.Categories.Where(e => e.DisplayName == NewEntryName).FirstOrDefault();

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
                        if (ImGui.Button($"Remove##removeNewParamName{i}", DPI.StandardButtonSize))
                        {
                            NewEntryParams.RemoveAt(i);
                            NewEntryParamsCount = NewEntryParams.Count;
                        }
                    }
                }
            }
        }
    }

    public static void RestoreDefault(ParamEditorScreen editor)
    {
        var sourceFolder = Path.Join(AppContext.BaseDirectory, "Assets", "PARAM", ProjectUtils.GetGameDirectory(editor.Project.Descriptor.ProjectType));
        var sourceFile = Path.Combine(sourceFolder, "Param Categories.json");

        if (File.Exists(sourceFile))
        {
            try
            {
                var filestring = File.ReadAllText(sourceFile);

                try
                {
                    editor.Project.Handler.ParamData.ParamCategories = JsonSerializer.Deserialize(filestring, ParamEditorJsonSerializerContext.Default.ParamCategoryResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog("[Smithbox] Failed to deserialize param categories", LogLevel.Error, LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read param categories", LogLevel.Error, LogPriority.High, e);
            }
        }
        else
        {
            TaskLogs.AddLog("[Smithbox] Failed to find default param categories for game", LogLevel.Error, LogPriority.High);
        }
    }

    public static void Write(ParamEditorScreen editor)
    {
        if (editor.Project.Descriptor.ProjectType == ProjectType.Undefined)
            return;

        var modResourceDir = Path.Join(editor.Project.Descriptor.ProjectPath, ".smithbox", "Assets", "PARAM", ProjectUtils.GetGameDirectory(editor.Project));
        var modResourcePath = Path.Combine(modResourceDir, "Param Categories.json");

        if (!Directory.Exists(modResourceDir))
        {
            Directory.CreateDirectory(modResourceDir);
        }

        try
        {
            string jsonString = JsonSerializer.Serialize(editor.Project.Handler.ParamData.ParamCategories, typeof(ParamCategoryResource), ProjectJsonSerializerContext.Default);
            var fs = new FileStream(modResourcePath, System.IO.FileMode.Create);
            var data = Encoding.ASCII.GetBytes(jsonString);
            fs.Write(data, 0, data.Length);
            fs.Flush();
            fs.Dispose();
        }
        catch (Exception ex)
        {
            TaskLogs.AddLog("[Smithbox] Failed to write project param categories", LogLevel.Error, LogPriority.High, ex);
        }

    }
}
