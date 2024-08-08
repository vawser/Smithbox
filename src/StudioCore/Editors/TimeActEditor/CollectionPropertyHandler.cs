using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Handles property edits for collection fields
/// </summary>
public class CollectionPropertyHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;
    private TimeActDecorator Decorator;
    private TimeActSelectionHandler SelectionHandler;

    public CollectionPropertyHandler(ActionManager editorActionManager, TimeActEditorScreen screen, TimeActDecorator decorator)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;
        Decorator = decorator;
        SelectionHandler = screen.SelectionHandler;

    }

    // TODO: actionize

    public void DuplicateTimeAct()
    {
        if (SelectionHandler.CurrentTimeAct == null)
            return;

        if (SelectionHandler.CurrentTimeActKey == -1)
            return;

        var curInternalFile = SelectionHandler.ContainerInfo.InternalFiles[SelectionHandler.CurrentTimeActKey];
        var newInternalFile = new InternalFileInfo(curInternalFile.Filepath, curInternalFile.TAE.Clone());

        TaskLogs.AddLog($"Name: {newInternalFile.Name}");
        TaskLogs.AddLog($"Filepath: {newInternalFile.Filepath}");
        TaskLogs.AddLog($"TAE.ID: {newInternalFile.TAE.ID}");

        int id = int.Parse(newInternalFile.Name.Substring(1));
        string newName = GetNewFileName(id);
        string newFilePath = newInternalFile.Filepath.Replace(newInternalFile.Name, newName);
        int newTaeID = GetNewTAEID(newInternalFile.TAE.ID);

        TaskLogs.AddLog($"New Name: {newName}");
        TaskLogs.AddLog($"New File Path: {newFilePath}");
        TaskLogs.AddLog($"New TAE ID: {newTaeID}");

        newInternalFile.Name = newName;
        newInternalFile.Filepath = newFilePath;
        newInternalFile.TAE.ID = newTaeID;

        SelectionHandler.ContainerInfo.InternalFiles.Insert(SelectionHandler.CurrentTimeActKey+1, newInternalFile);
        SelectionHandler.ResetOnTimeActChange();
    }

    public void DeleteTimeAct()
    {
        if (SelectionHandler.CurrentTimeAct == null)
            return;

        if (SelectionHandler.CurrentTimeActKey == -1)
            return;

        var curInternalFile = SelectionHandler.ContainerInfo.InternalFiles[SelectionHandler.CurrentTimeActKey];
        SelectionHandler.ContainerInfo.InternalFiles.Remove(curInternalFile);
        SelectionHandler.ResetOnTimeActChange();
    }

    public void DuplicateAnimation()
    {

    }
    public void DeleteAnimation()
    {

    }

    public void DuplicateEvent()
    {

    }

    public void DeleteEvent()
    {

    }

    public void OrderAnimation()
    {

    }

    public void OrderEvent()
    {

    }

    public enum OrderType
    {
        Up,
        Down,
        Top,
        Bottom,
        Sort
    }

    // Utility

    public string GetNewFileName(int id)
    {
        var trackedId = id;
        string newName = $"a{PadFileName(trackedId)}";

        // If there are matches, keep incrementing
        foreach (var file in SelectionHandler.ContainerInfo.InternalFiles)
        {
            if (file.Name == newName)
            {
                trackedId = trackedId + 1;
                newName = $"a{PadFileName(trackedId)}";
            }
        }

        return newName;
    }

    public string PadFileName(int id)
    {
        var str = "";

        if(id < 10)
        {
            str = "0";
        }

        return $"{str}{id}";
    }

    public int GetNewTAEID(int id)
    {
        int newID = id + 1;

        // If there are matches, keep incrementing
        foreach (var file in SelectionHandler.ContainerInfo.InternalFiles)
        {
            if (file.TAE.ID == newID)
            {
                newID = newID + 1;
            }
        }

        return newID;
    }
}
