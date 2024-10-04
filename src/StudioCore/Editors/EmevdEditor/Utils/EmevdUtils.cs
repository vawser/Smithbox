using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;
using static SoulsFormats.EMEVD.Instruction;
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

public static class EmevdUtils
{
    /// <summary>
    /// Does the EMEVD Editor support the currently loaded project?
    /// </summary>
    /// <returns></returns>
    public static bool SupportsEditor()
    {
        if (Smithbox.ProjectType is ProjectType.AC6)
            return true;

        return false;
    }

    /// <summary>
    /// Does the passed Instruction have an ArgDoc entry?
    /// </summary>
    public static bool HasArgDoc(Instruction ins)
    {
        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

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
    public static string GetArgDocName(Instruction ins)
    {
        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

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
    public static (List<ArgDoc>, List<object>) BuildArgumentList(Instruction ins)
    {
        var argList = new List<object>();
        var argDocList = new List<ArgDoc>();

        var classDoc = EmevdBank.InfoBank.Classes.Where(e => e.Index == ins.Bank).FirstOrDefault();

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
        catch(Exception ex)
        {
            // Clear the input so we don't lag out
            Smithbox.EditorHandler.EmevdEditor.Filters.EventFilterInput = "";

            TaskLogs.AddLog($"ArgDoc is incorrect for: {ins.Bank}[{ins.ID}] - ArgData Size: {ins.ArgData.Length}");
        }

        return (argDocList, argList);
    }
}
