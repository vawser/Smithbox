using ImGuiNET;
using SoulsFormats;
using StudioCore.Editors.TimeActEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.TimeActUtils;

namespace StudioCore.Tools;

/// <summary>
/// This will check every TAE, and if the event template fails, it will report the issue(s).
/// </summary>
public static class TimeActValidationTool
{
    public static bool HasFinished = false;

    public static void ValidateTAE()
    {
        SortedDictionary<int, string> errors = new();

        for (int i = 0; i < AnimationBank.FileBank.Count; i++)
        {
            var info = AnimationBank.FileBank.ElementAt(i).Key;
            var binder = AnimationBank.FileBank.ElementAt(i).Value;

            for (int k = 0; k < info.TimeActFiles.Count; k++)
            {
                TAE entry = info.TimeActFiles[k];

                TimeActUtils.ApplyTemplate(entry, TemplateType.Character);

                for (int j = 0; j < entry.Animations.Count; j++)
                {
                    TAE.Animation animEntry = entry.Animations[j];

                    for (int l = 0; l < animEntry.Events.Count; l++)
                    {
                        TAE.Event evt = animEntry.Events[l];

                        if(evt.Parameters == null)
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
