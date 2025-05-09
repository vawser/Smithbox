﻿using StudioCore.EmevdEditor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using static SoulsFormats.EMEVD;

namespace StudioCore.Editors.EmevdEditor.Framework;

/// <summary>
/// Holds the tool functions used by this editor.
/// </summary>
public class EmevdActionHandler
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;

    public EmevdActionHandler(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public void LogUnknownInstructions()
    {
        List<string> loggedInstructions = new List<string>();

        foreach (var (info, binder) in Screen.Project.EmevdBank.ScriptBank)
        {
            foreach (var evt in binder.Events)
            {
                var eventName = evt.Name;

                foreach (var ins in evt.Instructions)
                {
                    var insName = $"{ins.Bank}[{ins.ID}]";

                    if (!EmevdUtils.HasArgDoc(Screen, ins))
                    {
                        if (!loggedInstructions.Contains(insName))
                        {
                            loggedInstructions.Add(insName);
                            var output = DetermineUnknownParameters(ins, false);
                            TaskLogs.AddLog($"{insName}{output}\n");
                        }
                    }
                }
            }
        }
    }

    public string DetermineUnknownParameters(Instruction instruction, bool display = true)
    {
        var lineOuput = "";

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
