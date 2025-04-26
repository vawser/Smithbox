using HKLib.hk2018.hkaiCollisionAvoidance;
using HKLib.hk2018.hkAsyncThreadPool;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;
using static StudioCore.Editors.EmevdEditor.EMEDF;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the search filters used by the view classes.
/// </summary>
public class EmevdFilters
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdSelectionManager Selection;

    public EmevdFilters(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    public string FileFilterInput = "";
    public string EventFilterInput = "";
    public string InstructionFilterInput = "";

    private bool FileFilterExactMatch = false;
    private bool EventFilterExactMatch = false;
    private bool InstructionFilterExactMatch = false;

    private string FilterCommands_Event = "" +
        "The following commands can be used for special functionality:\n" +
        "ins: <input> - This will match your input against the instructions within the event.\n" +
        "prop: <input> - This will match your input against the properties within the instructions within the event.";

    private string FilterCommands_Instruction = "" +
        "The following commands can be used for special functionality:\n" +
        "prop: <input> - This will match your input against the properties within the instructions within the event.";

    /// <summary>
    /// Display the event filter UI
    /// </summary>
    public void DisplayFileFilterSearch()
    {
        ImGui.InputText($"Search##fileFilterSearch", ref FileFilterInput, 255);

        ImGui.SameLine();
        ImGui.Checkbox($"##fileFilterExactMatch", ref FileFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsFileFilterMatch(string text, string alias)
    {
        bool isValid = true;

        var input = FileFilterInput.ToLower();

        if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawText = text.ToLower();
            var rawAlias = alias.ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == rawText)
                    partTruth[i] = true;

                if (!FileFilterExactMatch)
                {
                    if (rawText.Contains(entry))
                        partTruth[i] = true;
                }

                if (entry == rawAlias)
                    partTruth[i] = true;

                if (!FileFilterExactMatch)
                {
                    if (rawAlias.Contains(entry))
                        partTruth[i] = true;
                }
            }

            // Only evaluate as true if all parts are true
            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Display the event filter UI
    /// </summary>
    public void DisplayEventFilterSearch()
    {
        ImGui.InputText($"Search##eventFilterSearch", ref EventFilterInput, 255);
        UIHelper.Tooltip(FilterCommands_Event);

        ImGui.SameLine();
        ImGui.Checkbox($"##eventFilterExactMatch", ref EventFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed text?
    /// </summary>
    public bool IsEventFilterMatch(EMEVD.Event evt)
    {
        var aliasText = EmevdUtils.GetDS2ItemAlias(evt);
        var text = $"{evt.ID} {evt.Name} {aliasText}";


        bool isValid = true;

        var input = EventFilterInput.ToLower();

        if (input != "" && text != null)
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            // Instruction filter
            if ( (input.StartsWith("ins:") || input.StartsWith("prop:") ) && input.Length > 4)
            {
                var overrideInput = EventFilterInput[4..].ToLower();

                // Retain full input string if we are passing along a property filter
                if(input.StartsWith("prop:"))
                {
                    overrideInput = input;

                    // Copy the input to this so the instruction list is filtered too
                    if (CFG.Current.EmevdEditor_PropagateFilterCommands)
                    {
                        InstructionFilterInput = overrideInput;
                    }
                }

                for (int i = 0; i < partTruth.Length; i++)
                {
                    foreach (var ins in evt.Instructions)
                    {
                        if (IsInstructionFilterMatch(ins, overrideInput))
                        {
                            partTruth[i] = true;
                        }
                    }
                }
            }
            // Normal filter
            else
            {
                var rawText = text.ToLower();

                for (int i = 0; i < partTruth.Length; i++)
                {
                    string entry = inputParts[i];

                    if (entry == rawText)
                        partTruth[i] = true;

                    if (!EventFilterExactMatch)
                    {
                        if (rawText.Contains(entry))
                            partTruth[i] = true;
                    }
                }
            }

            // Only evaluate as true if all parts are true
            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Display the instruction filter UI
    /// </summary>
    public void DisplayInstructionFilterSearch()
    {
        ImGui.InputText($"Search##instructionFilterSearch", ref InstructionFilterInput, 255);
        UIHelper.Tooltip(FilterCommands_Instruction);

        ImGui.SameLine();
        ImGui.Checkbox($"##instructionFilterExactMatch", ref InstructionFilterExactMatch);
        UIHelper.Tooltip("Filter will ignore partial matches when enabled.");
    }

    /// <summary>
    /// Is the search input an match for the passed Instruction
    /// </summary>
    public bool IsInstructionFilterMatch(Instruction ins, string overrideInput = "")
    {
        var name = $"{ins.Bank}[{ins.ID}]";
        var argDocName = EmevdUtils.GetArgDocName(ins);

        bool isValid = true;
        var input = InstructionFilterInput.ToLower();

        if(overrideInput != "")
        {
            input = overrideInput.ToLower();
        }

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            // Prop filter
            if (input.StartsWith("prop:") && input.Length > 4)
            {
                (var argumentDocs, var arguments) = EmevdUtils.BuildArgumentList(ins);

                if (argumentDocs != null && arguments != null)
                {
                    var propOverrideInput = input[5..].ToLower();

                    for (int i = 0; i < partTruth.Length; i++)
                    {
                        for (int j = 0; j < arguments.Count; j++)
                        {
                            var arg = arguments[j];
                            var argDoc = argumentDocs[j];

                            if (IsInstructionPropertyFilterMatch(arg, argDoc, propOverrideInput))
                            {
                                partTruth[i] = true;
                            }
                        }
                    }
                }
            }
            else
            {
                var rawName = name.ToLower();
                var rawArgName = argDocName.ToLower();

                for (int i = 0; i < partTruth.Length; i++)
                {
                    string entry = inputParts[i];

                    if (entry == rawName)
                        partTruth[i] = true;

                    if (entry == rawArgName)
                        partTruth[i] = true;

                    if (!InstructionFilterExactMatch)
                    {
                        if (rawName.Contains(entry))
                            partTruth[i] = true;

                        if (rawArgName.Contains(entry))
                            partTruth[i] = true;
                    }
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Is the search input an match for the passed Instruction argument value or name
    /// </summary>
    public bool IsInstructionPropertyFilterMatch(object value, ArgDoc doc, string overrideInput = "")
    {
        bool isValid = true;
        var input = InstructionFilterInput.ToLower();

        if (overrideInput != "")
        {
            input = overrideInput.ToLower();
        }

        if (input != "")
        {
            string[] inputParts = input.Split("+");
            bool[] partTruth = new bool[inputParts.Length];

            var rawDocName = doc.Name.ToLower();
            var rawValue = $"{value}".ToLower();

            for (int i = 0; i < partTruth.Length; i++)
            {
                string entry = inputParts[i];

                if (entry == rawDocName)
                    partTruth[i] = true;

                if (entry == rawValue)
                    partTruth[i] = true;

                if (!InstructionFilterExactMatch)
                {
                    if (rawDocName.Contains(entry))
                        partTruth[i] = true;

                    if (rawValue.Contains(entry))
                        partTruth[i] = true;
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                    isValid = false;
            }
        }

        return isValid;
    }
}


