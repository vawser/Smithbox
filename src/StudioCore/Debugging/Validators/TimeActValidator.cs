using Hexa.NET.ImGui;
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

        for (int i = 0; i < curProject.TimeActData.PrimaryCharacterBank.Entries.Count; i++)
        {
            var info = curProject.TimeActData.PrimaryCharacterBank.Entries.ElementAt(i).Key;
            var binder = curProject.TimeActData.PrimaryCharacterBank.Entries.ElementAt(i).Value;

            for (int k = 0; k < info.InternalFiles.Count; k++)
            {
                TAE entry = info.InternalFiles[k].TAE;

                ApplyTemplate(curProject.TimeActEditor, entry, TimeActTemplateType.Character);

                for (int j = 0; j < entry.Animations.Count; j++)
                {
                    TAE.Animation animEntry = entry.Animations[j];

                    for (int l = 0; l < animEntry.Events.Count; l++)
                    {
                        TAE.Event evt = animEntry.Events[l];

                        if (evt.Parameters == null)
                        {
                            if (!errors.ContainsKey(evt.Type))
                            {
                                var error = $"Bank: {entry.EventBank} - Event Size: {evt.GetParameterBytes(false).Length}";
                                errors.Add(evt.Type, error);
                            }
                        }
                    }
                }
            }
        }

        for (int i = 0; i < curProject.TimeActData.PrimaryObjectBank.Entries.Count; i++)
        {
            var info = curProject.TimeActData.PrimaryObjectBank.Entries.ElementAt(i).Key;
            var binder = curProject.TimeActData.PrimaryObjectBank.Entries.ElementAt(i).Value;

            for (int k = 0; k < info.InternalFiles.Count; k++)
            {
                TAE entry = info.InternalFiles[k].TAE;

                ApplyTemplate(curProject.TimeActEditor, entry, TimeActTemplateType.Character);

                for (int j = 0; j < entry.Animations.Count; j++)
                {
                    TAE.Animation animEntry = entry.Animations[j];

                    for (int l = 0; l < animEntry.Events.Count; l++)
                    {
                        TAE.Event evt = animEntry.Events[l];

                        if (evt.Parameters == null)
                        {
                            if (!errors.ContainsKey(evt.Type))
                            {
                                var error = $"Bank: {entry.EventBank} - Event Size: {evt.GetParameterBytes(false).Length}";
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
