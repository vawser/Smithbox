using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapModelInsightHelper
{
    public MapEditorView View;
    public ProjectEntry Project;

    public MapModelInsightHelper(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        Entries = new();
    }

    public Dictionary<string, MapModelInsightEntry> Entries { get; set; } = new();
    public MapModelInsightEntry SelectedDataEntry { get; set; }
    public MapFlverInsightEntry SelectedFlverEntry { get; set; }

    public void AddEntry(MapContainer container)
    {
        if (Entries != null && container != null)
        {
            if (!Entries.ContainsKey(container.Name))
            {
                Entries.Add(container.Name, new MapModelInsightEntry(container.Name, container));
            }
        }
    }

    public void ClearEntry(MapContainer container)
    {
        if (Entries != null && container != null)
        {
            if (Entries.ContainsKey(container.Name))
            {
                Entries.Remove(container.Name);
            }
        }
    }

    /// <param name="flverName">Resource name of the model (see ResourceName)</param>
    /// <param name="flverContainerPath">Project-relative path of the file the model was loaded from</param>
    /// <param name="textureName">Resource name of the texture</param>
    public void UpdateEntry(string flverName, string flverContainerPath, string textureName, IFlver flver, MTD mtd, MATBIN matbin, string materialStr)
    {
        if (Project == null)
            return;

        if (Project.Handler.ModelEditor == null)
            return;

        var curEntity = View.ViewportSelection.GetSelection().FirstOrDefault();

        if (curEntity == null)
            return;

        var mapEntity = (Entity)curEntity;

        var wrapperName = mapEntity.Name;

        if (Entries.ContainsKey(wrapperName))
        {
            var entry = Entries[wrapperName];

            if (!entry.Models.Any(e => e.Name == flverName))
            {
                var newModel = new MapFlverInsightEntry();
                newModel.Name = flverName;
                newModel.ContainerPath = flverContainerPath;
                newModel.FLVER2 = (FLVER2)flver;

                entry.Models.Add(newModel);
            }

            if (entry.Models.Any(e => e.Name == flverName))
            {
                var modelEntry = entry.Models.FirstOrDefault(e => e.Name == flverName);

                if (modelEntry != null)
                {
                    if (!modelEntry.Entries.Any(e => e.Name == textureName))
                    {
                        var newTexture = new MapTextureInsightEntry();
                        newTexture.Name = textureName;
                        newTexture.MTD = mtd;
                        newTexture.MATBIN = matbin;
                        newTexture.MaterialString = materialStr;

                        modelEntry.Entries.Add(newTexture);
                    }
                }
            }
        }
    }
}

public class MapModelInsightEntry
{
    public string MapID { get; set; }
    public MapContainer Container { get; set; }

    public System.Collections.Concurrent.ConcurrentBag<MapFlverInsightEntry> Models { get; set; } = new();

    public MapModelInsightEntry(string mapID, MapContainer container)
    {
        MapID = mapID;
        Container = container;
    }
}

public class MapFlverInsightEntry
{
    public string Name { get; set; }

    /// <summary>
    /// Project-relative path of the file the model was loaded from
    /// </summary>
    public string ContainerPath { get; set; }

    public FLVER2 FLVER2 { get; set; }

    public List<MapTextureInsightEntry> Entries { get; set; } = new();
}

public class MapTextureInsightEntry
{
    public string Name { get; set; }

    public string MaterialString { get; set; }

    public MTD MTD { get; set; }
    public MATBIN MATBIN { get; set; }
}
