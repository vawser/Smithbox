using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;

namespace StudioCore.EventScriptEditorNS;

public class UnknownInstructionFinder
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public UnknownInstructionFinder(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }


    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.CollapsingHeader("Unknown Instruction Finder"))
        {
            if (ImGui.Button("Search##unkSearch", defaultButtonSize))
            {
                LogUnknownInstructions();
            }

            UIHelper.SimpleHeader("##logOutput", "Logged Instructions", "", UI.Current.ImGui_AliasName_Text);

            var outputSize = new Vector2(windowWidth * 0.95f, 600);

            var buffer = UIHelper.GetTextInputBuffer(Output);
            ImGui.InputTextMultiline("##logOutputText", ref Output, buffer, outputSize);
        }
    }

    public List<string> LoggedInstructions = new List<string>();
    public string Output = "";

    public void LogUnknownInstructions()
    {
        Output = "";
        LoggedInstructions = new List<string>();

        List<Task> tasks = new();

        // Load all scripts
        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            var newTask = Project.EmevdData.PrimaryBank.LoadScript(entry.Key);
            tasks.Add(newTask);
        }

#if NET9_0_OR_GREATER
        Task.WaitAll(tasks);
#else
        Task.WaitAll(tasks.ToArray());
#endif

        string totalOutput = "";

        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            foreach (var evt in entry.Value.Events)
            {
                var eventName = evt.Name;

                foreach (var ins in evt.Instructions)
                {
                    var insName = $"{ins.Bank}[{ins.ID}]";

                    if (!EmevdUtils.HasArgDoc(Editor, ins))
                    {
                        if (!LoggedInstructions.Contains(insName))
                        {
                            LoggedInstructions.Add(insName);
                            totalOutput = $"{totalOutput}\n\n{DetermineUnknownParameters(ins, insName, false)}";
                        }
                    }
                }
            }
        }

        Output = totalOutput;
    }

    public string DetermineUnknownParameters(Instruction instruction, string insName, bool display = true)
    {
        var lineOuput = $"-- {insName}";

        // Display the byte contents as blocks of 4 bytes
        // Also show the potential int and float values
        byte[] blockArr = new byte[4];
        int blockIndex = 0;

        for (int i = 0; i < instruction.ArgData.Length; i++)
        {
            var block = instruction.ArgData[i];

            blockArr[blockIndex] = block;

            blockIndex++;

            if (i % 4 == 0)
            {
                int iValue = BitConverter.ToInt32(blockArr, 0);
                float fValue = BitConverter.ToSingle(blockArr, 0);
                short sValue_1 = BitConverter.ToInt16(blockArr[..2], 0);
                short sValue_2 = BitConverter.ToInt16(blockArr[2..], 0);

                var arrStr = "";
                foreach (var item in blockArr)
                {
                    if (item < 10)
                    {
                        arrStr += $"00{item} ";
                    }
                    else if (item < 100)
                    {
                        arrStr += $"0{item} ";
                    }
                    else
                    {
                        arrStr += $"{item} ";
                    }
                }

                var line = $"{arrStr}";
                if (display)
                {
                    UIHelper.WrappedText(line);
                }
                else
                {
                    lineOuput = $"{lineOuput}\n{line}";
                }

                blockIndex = 0;
                blockArr = new byte[4];
            }
        }

        return lineOuput;
    }
}
