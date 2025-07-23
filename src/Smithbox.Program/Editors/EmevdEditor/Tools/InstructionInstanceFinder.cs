using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.EventScriptEditorNS;

public class InstructionInstanceFinder
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public InstructionInstanceFinder(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.CollapsingHeader("Instruction Finder"))
        {
            ImGui.Text("Search Term");
            ImGui.SetNextItemWidth(windowWidth * 0.75f);
            ImGui.InputText("##instructionSearch", ref SearchTerm, 255);

            ImGui.SameLine();

            ImGui.Checkbox("##instructionMatchExact", ref MatchExact);
            UIHelper.Tooltip("If enabled, the search term must be an exact match.");

            if (ImGui.Button("Search##instructionSearch", defaultButtonSize))
            {
                FindInstructionInstances();
            }

            UIHelper.SimpleHeader("##instructionInstances", "Instruction Instances", "", UI.Current.ImGui_AliasName_Text);

            ImGui.BeginChild("instructionResults", new Vector2(windowWidth, 600));
            if (Results.Count > 0)
            {
                for (int i = 0; i < Results.Count; i++)
                {
                    var entry = Results[i];

                    var instructionKey = $"{entry.Instruction.Bank}[{entry.Instruction.ID}";

                    var displayName = $"{entry.FileEntry.Filename}: {entry.Event.ID}: {entry.InstructionKey} - {entry.InstructionAlias}";

                    if (ImGui.Selectable($"{displayName}##instructionInstance{i}", entry == SelectedResult))
                    {
                        SelectedResult = entry;
                        Editor.Selection.SelectLoadedFile(entry.FileEntry);
                        Editor.Selection.SelectEvent(entry.Event, entry.EventIndex);
                        Editor.Selection.SelectInstruction(entry.Instruction, entry.InstructionIndex);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectNextResult)
                    {
                        SelectNextResult = false;
                        SelectedResult = entry;
                        Editor.Selection.SelectLoadedFile(entry.FileEntry);
                        Editor.Selection.SelectEvent(entry.Event, entry.EventIndex);
                        Editor.Selection.SelectInstruction(entry.Instruction, entry.InstructionIndex);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectNextResult = true;
                    }
                }
            }
            ImGui.EndChild();
        }
    }

    public string SearchTerm = "";
    public bool MatchExact = false;
    public List<EmevdSearchResult> Results = new();
    public EmevdSearchResult SelectedResult;
    public bool SelectNextResult = false;

    public void FindInstructionInstances()
    {
        Results = new();

        List<Task> tasks = new();

        // Load all scripts
        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            var newTask = Project.EmevdData.PrimaryBank.LoadScript(entry.Key);
            tasks.Add(newTask);
        }

        Task.WaitAll(tasks);

        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            int index = 0;
            foreach (var evt in entry.Value.Events)
            {
                int insIndex = 0;
                foreach (var ins in evt.Instructions)
                {
                    var addResult = false;

                    var instructionKey = $"{ins.Bank}[{ins.ID}]";
                    var instructionAlias = GetInstructionAlias(ins);


                    if (MatchExact)
                    {
                        if (SearchTerm == instructionKey || SearchTerm == instructionAlias)
                        {
                            addResult = true;
                        }
                    }
                    else
                    {
                        if (instructionKey.ToLower().Contains(SearchTerm.ToLower()) || instructionAlias.ToLower().Contains(SearchTerm.ToLower()))
                        {
                            addResult = true;
                        }
                    }

                    if (addResult)
                    {
                        var newResult = new EmevdSearchResult();
                        newResult.FileEntry = entry.Key;
                        newResult.EMEVD = entry.Value;
                        newResult.Event = evt;
                        newResult.EventIndex = index;
                        newResult.Instruction = ins;
                        newResult.InstructionIndex = insIndex;
                        newResult.InstructionKey = instructionKey;
                        newResult.InstructionAlias = instructionAlias;

                        Results.Add(newResult);
                    }

                    insIndex++;
                }

                index++;
            }
        }
    }

    public string GetInstructionAlias(EMEVD.Instruction ins)
    {
        var classStr = "Unknown";
        var insStr = "Unknown";
        var argsStr = "";

        foreach (var classEntry in Project.EmevdData.PrimaryBank.InfoBank.Classes)
        {
            if (ins.Bank == classEntry.Index)
            {
                classStr = classEntry.Name;

                foreach (var insEntry in classEntry.Instructions)
                {
                    if (ins.ID == insEntry.Index)
                    {
                        insStr = insEntry.Name;

                        for (int i = 0; i < insEntry.Arguments.Length; i++)
                        {
                            var argEntry = insEntry.Arguments[i];
                            string separator = ", ";

                            if (i == insEntry.Arguments.Length - 1)
                            {
                                separator = "";
                            }

                            argsStr = $"{argsStr}{argEntry.Name}{separator}";
                        }
                    }
                }

            }
        }

        return insStr;
    }
}