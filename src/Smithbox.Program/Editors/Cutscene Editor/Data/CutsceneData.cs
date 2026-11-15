#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// Holds the data for the Cutscene Editor: the project's cutscene binders, each read on first use.
/// </summary>
public class CutsceneData : IDisposable
{
    public ProjectEntry Project;

    /// <summary>Every cutscene binder of the project, by name</summary>
    public List<CutsceneFile> Files { get; private set; } = new();

    public CutsceneData(ProjectEntry project)
    {
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Yield();

        try
        {
            Files = Project.Locator.FileDictionary.Entries
                .Where(e => e.Extension is "cutscenebnd" or "remobnd")
                .GroupBy(e => e.Filename, StringComparer.OrdinalIgnoreCase)
                .Select(g => new CutsceneFile(g.First()))
                .OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("CUTSCENE_Data_Setup_FAIL"), e);
            return false;
        }

        Smithbox.Log(this, LOC.Get("CUTSCENE_Data_Setup_PASS", Files.Count));
        return true;
    }

    public void Dispose()
    {
        Files = new();
    }
}
