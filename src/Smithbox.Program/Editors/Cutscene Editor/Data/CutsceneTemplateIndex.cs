#nullable enable
using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// One event of a resource type, found in some cutscene of the project, to make new events of that type from: its
/// resource, the event and its timeline's parameters. Every resource type keeps the same parameter set in the vanilla
/// files (Claude/Tools/MqbFieldSurvey), so any one of them is a fair template.
/// </summary>
public sealed record CutsceneTemplate(int Type, string Source, MQB.Resource Resource, MQB.Event Event,
    List<MQB.Parameter> TimelineParameters);

/// <summary>
/// The templates of the project's cutscenes, by resource type, read in the background the first time they are asked
/// for. Only the MQBs are read.
/// </summary>
public class CutsceneTemplateIndex
{
    private readonly ProjectEntry Project;
    private readonly Func<IReadOnlyList<CutsceneFile>> GetFiles;
    private Task<Dictionary<int, CutsceneTemplate>>? Build;

    public CutsceneTemplateIndex(ProjectEntry project, Func<IReadOnlyList<CutsceneFile>> getFiles)
    {
        Project = project;
        GetFiles = getFiles;
    }

    public bool IsReady => Build is { IsCompletedSuccessfully: true };
    public bool IsBuilding => Build is { IsCompleted: false };

    /// <summary>Starts reading the templates if they haven't been</summary>
    public void Start()
    {
        if (Build != null)
            return;

        var files = GetFiles().ToList();
        var project = Project;

        Build = Task.Run(() =>
        {
            var templates = new Dictionary<int, CutsceneTemplate>();
            foreach (var file in files)
            {
              // A loaded cutscene's MQB can be edited meanwhile on the main thread: a file that changes under the read
              // is skipped rather than ending the index
              try
              {
                var mqb = file.ReadMqbOnly(project);
                if (mqb == null)
                    continue;

                foreach (var cut in mqb.Cuts)
                {
                    foreach (var timeline in cut.Timelines)
                    {
                        foreach (var e in timeline.Events)
                        {
                            if (e.ResourceIndex < 0 || e.ResourceIndex >= mqb.Resources.Count)
                                continue;

                            var resource = mqb.Resources[e.ResourceIndex];
                            if (!CutsceneMqbOps.CanTemplate(resource.Type) || templates.ContainsKey(resource.Type))
                                continue;

                            templates[resource.Type] = new CutsceneTemplate(resource.Type, $"{file.Name} {cut.Name}",
                                CutsceneMqbOps.Clone(resource), CutsceneMqbOps.Clone(e),
                                timeline.Parameters.Select(CutsceneMqbOps.Clone).ToList());
                        }
                    }
                }
              }
              catch (InvalidOperationException)
              {
              }
            }

            return templates;
        });
    }

    /// <summary>The templates by type name, empty until they are read</summary>
    public IEnumerable<CutsceneTemplate> Templates => IsReady
        ? Build!.Result.Values.OrderBy(t => CutsceneResourceTypes.Name(t.Type), StringComparer.OrdinalIgnoreCase)
        : Enumerable.Empty<CutsceneTemplate>();
}
