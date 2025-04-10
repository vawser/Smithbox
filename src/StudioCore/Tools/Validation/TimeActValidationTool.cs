
using SoulsFormats;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.TimeActEditor.Utils.TimeActUtils;

namespace StudioCore.Tools.Validation;

/// <summary>
/// This will check every TAE, and if the event template fails, it will report the issue(s).
/// </summary>
public static class TimeActValidationTool
{
    public static bool HasFinished = false;

    public static void ValidateTAE()
    {
        SortedDictionary<int, string> errors = new();

        for (int i = 0; i < TimeActBank.FileChrBank.Count; i++)
        {
            var info = TimeActBank.FileChrBank.ElementAt(i).Key;
            var binder = TimeActBank.FileChrBank.ElementAt(i).Value;

            for (int k = 0; k < info.InternalFiles.Count; k++)
            {
                TAE entry = info.InternalFiles[k].TAE;

                ApplyTemplate(entry, TimeActTemplateType.Character);

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

        for (int i = 0; i < TimeActBank.FileObjBank.Count; i++)
        {
            var info = TimeActBank.FileObjBank.ElementAt(i).Key;
            var binder = TimeActBank.FileObjBank.ElementAt(i).Value;

            for (int k = 0; k < info.InternalFiles.Count; k++)
            {
                TAE entry = info.InternalFiles[k].TAE;

                ApplyTemplate(entry, TimeActTemplateType.Character);

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
