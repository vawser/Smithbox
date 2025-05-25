using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.EventScriptEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using static SoulsFormats.EMEVD;
using static SoulsFormats.EMEVD.Instruction;
using static StudioCore.EventScriptEditorNS.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

public static class EmevdUtils
{
    /// <summary>
    /// Does the EMEVD Editor support the currently loaded project?
    /// </summary>
    /// <returns></returns>
    public static bool SupportsEditor()
    {
        return true;
    }

    /// <summary>
    /// Does the passed Instruction have an ArgDoc entry?
    /// </summary>
    public static bool HasArgDoc(EmevdEditorScreen editor, Instruction ins)
    {
        var classDoc = editor.Project.EmevdData.PrimaryBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if (classDoc == null)
        {
            return false;
        }

        var instructionDoc = classDoc[ins.ID];

        if (instructionDoc == null)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Get ArgDoc Name for instruction
    /// </summary>
    public static string GetArgDocName(EmevdEditorScreen editor, Instruction ins)
    {
        var classDoc = editor.Project.EmevdData.PrimaryBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if (classDoc == null)
        {
            return "";
        }

        var instructionDoc = classDoc[ins.ID];

        if (instructionDoc == null)
        {
            return "";
        }

        return instructionDoc.Name;
    }

    /// <summary>
    /// Constructs two lists with the ArgDoc and raw ArgData for the current Instruction.
    /// </summary>
    public static (List<ArgDoc>, List<object>) BuildArgumentList(EmevdEditorScreen editor, Instruction ins)
    {
        var argList = new List<object>();
        var argDocList = new List<ArgDoc>();

        var classDoc = editor.Project.EmevdData.PrimaryBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

        if (classDoc == null)
        {
            return (argDocList, argList);
        }

        var instructionDoc = classDoc[ins.ID];

        if (instructionDoc == null)
        {
            return (argDocList, argList);
        }

        var data = ins.ArgData;

        List<ArgType> argTypes = instructionDoc.Arguments.Select(arg => arg.Type == 8 ? ArgType.UInt32 : (ArgType)arg.Type).ToList();

        try
        {
            var argObjects = ins.UnpackArgs(argTypes);
            for (int i = 0; i < instructionDoc.Arguments.Length; i++)
            {
                var entry = instructionDoc.Arguments[i];
                var obj = argObjects[i];

                argDocList.Add(entry);
                argList.Add(obj);
            }
        }
        catch(Exception e)
        {
            // Clear the input so we don't lag out
            editor.Filters.EventFilterInput = "";

            TaskLogs.AddLog($"ArgDoc is incorrect for: {ins.Bank}[{ins.ID}] - ArgData Size: {ins.ArgData.Length}", LogLevel.Error, Tasks.LogPriority.High, e);
        }

        return (argDocList, argList);
    }

    public static string GetDS2ItemAlias(EmevdEditorScreen editor, Event evt)
    {
        if (editor.BaseEditor.ProjectManager.SelectedProject == null)
            return "";

        var curProject = editor.BaseEditor.ProjectManager.SelectedProject;

        if (curProject.ParamEditor == null)
            return "";

        var eventName = evt.Name;
        var itemName = curProject.ParamData.PrimaryBank.GetParamFromName("ItemParam");

        if (itemName != null)
        {

            var itemRow = itemName.Rows.Where(e => e.ID == (int)evt.ID).FirstOrDefault();

            if (itemRow != null)
                eventName = itemRow.Name;
        }

        return eventName;
    }
}
