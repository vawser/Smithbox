using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorBinderList
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorBinderList(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        DisplayHeader();
        DisplayBinderList();
    }
    public void DisplayHeader()
    {
        var curInput = Editor.Filters.BinderListInput;
        ImGui.InputText($"Search##searchBar_BinderList", ref curInput, 255);
        if(ImGui.IsItemDeactivatedAfterEdit())
        {
            Editor.Filters.BinderListInput = curInput;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##searchIgnoreCase_BinderList", ref Editor.Filters.BinderListInput_IgnoreCase);
        UIHelper.Tooltip("If enabled, the search will ignore case.");
    }

    private void DisplayBinderList()
    {
        ImGui.BeginChild("behaviorBinderList");

        int index = 0;

        foreach(var entry in Project.BehaviorData.PrimaryBank.Binders)
        {
            var fileEntry = entry.Key;
            var binderContents = entry.Value;

            var isSelected = Editor.Selection.IsBinderSelected(fileEntry);

            var aliasName = "";

            if (Project.Aliases.Characters.Any(e => e.ID == fileEntry.Filename))
            {
                aliasName = Project.Aliases.Characters.FirstOrDefault(e => e.ID == fileEntry.Filename).Name;
            }

            if (Editor.Filters.IsBasicMatch(
                ref Editor.Filters.BinderListInput, Editor.Filters.BinderListInput_IgnoreCase,
                fileEntry.Filename, aliasName))
            {
                if (ImGui.Selectable($"{fileEntry.Filename}##fileEntry{index}", isSelected))
                {
                    Editor.Selection.SelectBinder(fileEntry, binderContents);
                    if (binderContents == null)
                    {
                        Editor.Selection.LoadBinder = true;
                    }
                }

                if(aliasName != "")
                    UIHelper.DisplayAlias(aliasName);
            }

            index++;
        }

        ImGui.EndChild();
    }

    public void Update()
    {
        if(Editor.Selection.LoadBinder)
        {
            if (Project.BehaviorData.PrimaryBank.Binders.Any(e => e.Key == Editor.Selection.SelectedFileEntry))
            {
                Task<bool> loadTask = Project.BehaviorData.PrimaryBank.LoadBinder(Editor.Selection.SelectedFileEntry);
                Task.WaitAll(loadTask);

                var entry = Project.BehaviorData.PrimaryBank.Binders[Editor.Selection.SelectedFileEntry];

                Editor.Selection.SelectedBinderContents = entry;
                Editor.Selection.LoadBinder = false;
            }
        }
    }
}
