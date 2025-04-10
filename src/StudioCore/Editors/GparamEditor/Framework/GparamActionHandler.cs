using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Editor;
using StudioCore.Editors.GparamEditor.Actions;
using StudioCore.Editors.GparamEditor.Data;
using StudioCore.Editors.GparamEditor.Utils;
using StudioCore.GraphicsEditor;
using System.IO;

namespace StudioCore.Editors.GparamEditor;

public class GparamActionHandler
{
    private GparamEditorScreen Screen;
    private ActionManager EditorActionManager;

    private string _copyFileNewName = "";

    public GparamActionHandler(GparamEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
    }
    public void DeleteValueRow()
    {
        if (Screen.Selection.CanAffectSelection())
        {
            var action = new GparamRemoveValueRow(Screen);
            EditorActionManager.ExecuteAction(action);
        }
    }
    public void DuplicateValueRow()
    {
        if (Screen.Selection.CanAffectSelection())
        {
            var action = new GparamDuplicateValueRow(Screen);
            EditorActionManager.ExecuteAction(action);
        }
    }

    /// <summary>
    /// Remove target GPARAM file from project
    /// </summary>
    /// <param name="info"></param>
    public void RemoveGparamFile(GparamParamBank.GparamInfo info)
    {
        string filePath = info.Path;
        string baseFileName = info.Name;

        filePath = filePath.Replace($"{Smithbox.GameRoot}", $"{Smithbox.ProjectRoot}");

        if (File.Exists(filePath))
        {
            TaskLogs.AddLog($"{baseFileName} was removed from your project.");
            File.Delete(filePath);
        }
        else
        {
            TaskLogs.AddLog($"{baseFileName} does not exist within your project.");
        }

        GparamParamBank.LoadGraphicsParams();
    }

    /// <summary>
    /// Copy and rename target GPARAM file
    /// </summary>
    /// <param name="info"></param>
    public void CopyGparamFile(GparamParamBank.GparamInfo info)
    {
        string filePath = info.Path;
        string baseFileName = info.Name;
        string tryFileName = _copyFileNewName;

        string newFilePath = filePath.Replace(baseFileName, tryFileName);

        // If the original is in the root dir, change the path to mod
        newFilePath = newFilePath.Replace($"{Smithbox.GameRoot}", $"{Smithbox.ProjectRoot}");

        if (!File.Exists(newFilePath))
        {
            File.Copy(filePath, newFilePath);
        }
        else
        {
            TaskLogs.AddLog($"{newFilePath} already exists!", LogLevel.Warning);
        }

        GparamParamBank.LoadGraphicsParams();
    }

    public void DisplayCopyFileNameInput(string name)
    {
        // Copy
        if (_copyFileNewName == "")
            _copyFileNewName = name;

        ImGui.InputText("##copyInputName", ref _copyFileNewName, 255);
    }

    /// <summary>
    /// Duplicate target GPARAM file, increment sub ID if valid
    /// </summary>
    public void DuplicateGparamFile()
    {
        bool isValidFile = false;
        string filePath = Screen.Selection._selectedGparamInfo.Path;
        string baseFileName = Screen.Selection._selectedGparamInfo.Name;
        string tryFileName = Screen.Selection._selectedGparamInfo.Name;

        do
        {
            string currentfileName = GparamUtils.CreateDuplicateFileName(tryFileName);
            string newFilePath = filePath.Replace(baseFileName, currentfileName);

            // If the original is in the root dir, change the path to mod
            newFilePath = newFilePath.Replace($"{Smithbox.GameRoot}", $"{Smithbox.ProjectRoot}");

            if (!File.Exists(newFilePath))
            {
                File.Copy(filePath, newFilePath);
                isValidFile = true;
            }
            else
            {
                TaskLogs.AddLog($"{newFilePath} already exists!", LogLevel.Warning);
                tryFileName = currentfileName;
            }
        }
        while (!isValidFile);

        GparamParamBank.LoadGraphicsParams();
    }
}
