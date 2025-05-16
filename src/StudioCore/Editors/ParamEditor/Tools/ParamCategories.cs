using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;

namespace StudioCore.Editors.ParamEditor.Tools;

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
        var categories = editor.Project.ParamCategories;

        if (categories == null)
        {
            UIHelper.WrappedText("No param categories found.");

            return;
        }

        var sectionWidth = ImGui.GetWindowWidth();
        var sectionHeight = ImGui.GetWindowHeight();
        var defaultButtonSize = new Vector2(sectionWidth * 0.975f, 32);
        var thirdButtonSize = new Vector2((sectionWidth * 0.975f / 3), 32);

        UIHelper.WrappedText("Create or modify project-specific param categories.");
        UIHelper.WrappedText("");

        ImGui.Separator();

        if (ImGui.Button("New Entry", new Vector2(sectionWidth * 0.5f, 24)))
        {
            isNewEntryMode = true;
            isEditEntryMode = false;

            NewEntryName = "";
            NewEntryParamsCount = 1;
            NewEntryParams = new List<string>() { "" };
        }
        UIHelper.Tooltip("Create a new param category.");

        ImGui.SameLine();
        if (ImGui.Button("Save Changes", new Vector2(sectionWidth * 0.5f, 24)))
        {
            Write(editor);
            isNewEntryMode = false;
            isEditEntryMode = false;
        }
        UIHelper.Tooltip("Permanently save the current param categories to your project's .smithbox folder, so they persist across sessions.");

        if (ImGui.Button("Edit Selected Entry", new Vector2(sectionWidth * 0.5f, 24)))
        {
            isNewEntryMode = false;
            isEditEntryMode = true;
            isInitialEditMode = true;
        }
        UIHelper.Tooltip("Edit the currently selected param category.");

        ImGui.SameLine();
        if (ImGui.Button("Delete Selected Entry", new Vector2(sectionWidth * 0.5f, 24)))
        {
            editor.Project.ParamCategories.Categories.Remove(_selectedUserCategory);
            _selectedUserCategory = null;
            isNewEntryMode = false;
            isEditEntryMode = false;
        }
        UIHelper.Tooltip("Delete the currently selected param category.");

        if (ImGui.Button("Restore Base Categories", new Vector2(sectionWidth * 1.0f, 24)))
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

        foreach(var category in editor.Project.ParamCategories.Categories)
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
                    if (ImGui.Button($"Remove##removeNewParamName{i}"))
                    {
                        NewEntryParams.RemoveAt(i);
                        NewEntryParamsCount = NewEntryParams.Count;
                    }
                }
            }

            ImGui.Text("");

            if (ImGui.Button("Expand List", new Vector2(sectionWidth * 0.5f, 24)))
            {
                NewEntryParams.Add("");
                NewEntryParamsCount++;
            }
            UIHelper.Tooltip("Add another param entry to fill.");

            ImGui.SameLine();

            if (ImGui.Button("Finalize Entry", new Vector2(sectionWidth * 0.5f, 24)))
            {
                isNewEntryMode = false;

                var newCategoryEntry = new ParamCategoryEntry();
                newCategoryEntry.DisplayName = NewEntryName;
                newCategoryEntry.Params = NewEntryParams;

                editor.Project.ParamCategories.Categories.Add(newCategoryEntry);
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

                if (ImGui.Button("Expand List", new Vector2(sectionWidth * 0.5f, 24)))
                {
                    NewEntryParams.Add("");
                    NewEntryParamsCount++;
                }
                UIHelper.Tooltip("Add another param entry to fill.");

                ImGui.SameLine();

                if (ImGui.Button("Finalize Entry", new Vector2(sectionWidth * 0.5f, 24)))
                {
                    isEditEntryMode = false;

                    var curEntry = editor.Project.ParamCategories.Categories.Where(e => e.DisplayName == NewEntryName).FirstOrDefault();

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

    public static void RestoreDefault(ParamEditorScreen editor)
    {
        var sourceFolder = $@"{AppContext.BaseDirectory}\Assets\PARAM\{ProjectUtils.GetGameDirectory(editor.Project.ProjectType)}";
        var sourceFile = Path.Combine(sourceFolder, "Categories.json");

        if (File.Exists(sourceFile))
        {
            try
            {
                var filestring = File.ReadAllText(sourceFile);

                try
                {
                    var options = new JsonSerializerOptions();
                    editor.Project.ParamCategories = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.ParamCategoryResource);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog("[Smithbox] Failed to deserialize param categories", LogLevel.Error, Tasks.LogPriority.High, e);
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to read param categories", LogLevel.Error, Tasks.LogPriority.High, e);
            }
        }
    }

    public static void Write(ParamEditorScreen editor)
    {
        if (editor.Project.ProjectType == ProjectType.Undefined)
            return;

        var modResourceDir = $"{editor.Project.ProjectPath}\\.smithbox\\Assets\\PARAM\\{ProjectUtils.GetGameDirectory(editor.Project)}\\";
        var modResourcePath = Path.Combine(modResourceDir, "Categories.json");

        if (!Directory.Exists(modResourceDir))
        {
            Directory.CreateDirectory(modResourceDir);
        }

        if (Directory.Exists(modResourceDir))
        {
            string jsonString = JsonSerializer.Serialize(editor.Project.ParamCategories, typeof(ParamCategoryResource), SmithboxSerializerContext.Default);

            try
            {
                var fs = new FileStream(modResourcePath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Failed to write project param categories:\n{ex}", LogLevel.Error);
            }
        }
    }
}
