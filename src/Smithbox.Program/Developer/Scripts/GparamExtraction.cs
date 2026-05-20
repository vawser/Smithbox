using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.GparamEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Developer;

public static class GparamExtraction
{
    public static void Apply()
    {
        var curProject = Smithbox.Orchestrator.SelectedProject;

        LoadGparams(curProject);
    }

    public static void LoadGparams(ProjectEntry curProject)
    {
        Dictionary<FileDictionaryEntry, GPARAM> gparamEntries = new();

        foreach (var entry in curProject.Locator.GparamFiles.Entries)
        {
            try
            {
                var gparamData = curProject.VFS.FS.ReadFileOrThrow(entry.Path);

                try
                {
                    var gparam = GPARAM.Read(gparamData);

                    gparamEntries.Add(entry, gparam);
                }
                catch (Exception e)
                {
                    Smithbox.LogError<DeveloperPanel>($"[Graphics Param Editor] Failed to read {entry.Path} as GPARAM for {entry.Filename}.", e);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError<DeveloperPanel>($"[Graphics Param Editor] Failed to read {entry.Path} from VFS for {entry.Filename}.", e);
            }
        }

        ExtractGparamData(curProject, gparamEntries);
    }

    // Build def bank for the individual fields
    public static void ExtractGparamData(ProjectEntry curProject, Dictionary<FileDictionaryEntry, GPARAM> entries)
    {
        var annotationEntries = new List<GparamAnnotationEntry>();

        foreach (var entry in entries)
        {
            foreach (var param in entry.Value.Params)
            {
                if (!annotationEntries.Any(e => e.ID == param.Key))
                {
                    var newAnnotation = new GparamAnnotationEntry();
                    newAnnotation.ID = param.Key;
                    newAnnotation.Name = param.Key;
                    newAnnotation.Description = "";
                    newAnnotation.Fields = new();

                    annotationEntries.Add(newAnnotation);
                }

                foreach (var field in param.Fields)
                {
                    if (annotationEntries.Any(e => e.ID == param.Key))
                    {
                        var curAnnotation = annotationEntries.FirstOrDefault(e => e.ID == param.Key);

                        if (curAnnotation != null)
                        {
                            if (!curAnnotation.Fields.Any(e => e.ID == field.Key))
                            {
                                var newFieldEntry = new GparamAnnotationFieldEntry();
                                newFieldEntry.ID = field.Key;
                                newFieldEntry.Name = field.Key;
                                newFieldEntry.Description = "";
                                newFieldEntry.Type = $"{field.GetType().Name}";

                                curAnnotation.Fields.Add(newFieldEntry);
                            }
                        }
                    }
                }
            }
        }

        var outputDir = @"C:\Users\benja\Programming\C#\Smithbox\src\Smithbox.Data\Assets\GPARAM\temp";

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
            IncludeFields = true
        };

        int id = 0;
        foreach (var entry in annotationEntries)
        {
            var json = JsonSerializer.Serialize(entry, options);
            File.WriteAllText(Path.Combine(outputDir, $"{id}.json"), json);

            id++;
        }
    }
}
