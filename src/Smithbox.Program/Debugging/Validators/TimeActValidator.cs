using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.Utils.TimeActUtils;

namespace StudioCore.DebugNS;

/// <summary>
/// This will check every TAE, and if the event template fails, it will report the issue(s).
/// </summary>
public static class TimeActValidator
{
    public static bool HasFinished = false;

    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (project == null)
            return;

        if (project.TimeActEditor == null)
            return;

        var buttonSize = new Vector2(400, 32);

        ImGui.Text("This tool will validate the Time Act files for the current project by loading all TAE files.");
        ImGui.Text("");

        if (HasFinished)
        {
            ImGui.Text("Validation has finished.");
            ImGui.Text("");
        }

        if (ImGui.Button("Validate TAE", buttonSize))
        {
            ValidateTAE(baseEditor,  project);
        }
    }

    public static void ValidateTAE(Smithbox baseEditor, ProjectEntry curProject)
    {
        SortedDictionary<int, string> errors = new();

        var timeActEditor = curProject.TimeActEditor;

        foreach (var entry in curProject.TimeActData.PrimaryBank.Entries)
        {
            var file = entry.Key;
            var binder = entry.Value;

            for (int k = 0; k < binder.Files.Count; k++)
            {
                var curFile = timeActEditor.Selection.SelectedBinder.Files.ElementAt(k);
                var binderFile = curFile.Key;
                var taeEntry = curFile.Value;

                ApplyTemplate(curProject.TimeActEditor, taeEntry, TimeActTemplateType.Character);

                for (int j = 0; j < taeEntry.Animations.Count; j++)
                {
                    TAE.Animation animEntry = taeEntry.Animations[j];

                    for (int l = 0; l < animEntry.Events.Count; l++)
                    {
                        TAE.Event evt = animEntry.Events[l];

                        if (evt.Parameters == null)
                        {
                            if (!errors.ContainsKey(evt.Type))
                            {
                                var error = $"Bank: {taeEntry.EventBank} - Event Size: {evt.GetParameterBytes(false).Length}";
                                errors.Add(evt.Type, error);
                            }
                        }
                    }
                }
            }
        }

        foreach (var (id, name) in errors)
        {
            TaskLogs.AddLog($"Event Type: {id} - {name}");
        }
        HasFinished = true;
    }
}
