using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;

namespace StudioCore.EventScriptEditorNS;

public class ValueInstanceFinder
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public ValueInstanceFinder(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.CollapsingHeader("Value Finder"))
        {
            ImGui.Text("Search Term");
            ImGui.SetNextItemWidth(windowWidth * 0.75f);
            ImGui.InputText("##valueSearch", ref SearchTerm, 255);

            ImGui.SameLine();

            ImGui.Checkbox("##valueMatchExact", ref MatchExact);
            UIHelper.Tooltip("If enabled, the search term must be an exact match.");

            if (ImGui.Button("Search##valueSearch", defaultButtonSize))
            {
                FindValueInstances();
            }

            UIHelper.SimpleHeader("##valueInstances", "Value Instances", "", UI.Current.ImGui_AliasName_Text);

            ImGui.BeginChild("valueResults", new Vector2(windowWidth, 600));
            if (Results.Count > 0)
            {
                for (int i = 0; i < Results.Count; i++)
                {
                    var entry = Results[i];

                    var instructionKey = $"{entry.Instruction.Bank}[{entry.Instruction.ID}";
                    var displayName = $"{entry.FileEntry.Filename}: {entry.Event.ID}: {entry.InstructionKey} - {entry.InstructionAlias}: {entry.Value}";

                    if (ImGui.Selectable($"{displayName}##valueInstance{i}", entry == SelectedResult))
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

    public void FindValueInstances()
    {
        Results = new();

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

        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            int index = 0;
            foreach (var evt in entry.Value.Events)
            {
                int insIndex = 0;
                foreach (var ins in evt.Instructions)
                {
                    var strVal = "";
                    var addResult = false;

                    var instructionKey = $"{ins.Bank}[{ins.ID}]";
                    var instructionAlias = GetInstructionAlias(ins);

                    if (EmevdUtils.HasArgDoc(Editor, ins))
                    {
                        var (ArgumentDocs, Arguments) = EmevdUtils.BuildArgumentList(Editor, ins);

                        for (int i = 0; i < Arguments.Count; i++)
                        {
                            var argDoc = ArgumentDocs[i];
                            strVal = $"{Arguments[i]}";

                            if (MatchExact)
                            {
                                if (SearchTerm == strVal || SearchTerm == strVal)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (strVal.ToLower().Contains(SearchTerm.ToLower()) || strVal.ToLower().Contains(SearchTerm.ToLower()))
                                {
                                    addResult = true;
                                }
                            }
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
                        newResult.Value = strVal;

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

