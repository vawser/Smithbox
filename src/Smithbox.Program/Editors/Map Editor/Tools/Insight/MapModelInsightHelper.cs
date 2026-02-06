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

    public Dictionary<string, MapModelInsightEntry> Entries { get; set; }
    public MapModelInsightEntry SelectedDataEntry { get; set; }
    public MapFlverInsightEntry SelectedFlverEntry { get; set; }

    public void AddEntry(MapContainer container)
    {
        if (!Entries.ContainsKey(container.Name))
        {
            Entries.Add(container.Name, new MapModelInsightEntry(container.Name, container));
        }
    }

    public void ClearEntry(MapContainer container)
    {
        if (Entries.ContainsKey(container.Name))
        {
            Entries.Remove(container.Name);
        }
    }

    public void UpdateEntry(string flverVirtPath, string texVirtPath, IFlver flver, MTD mtd, MATBIN matbin, string materialStr)
    {
        if (Project == null)
            return;

        if (Project.Handler.ModelEditor == null)
            return;

        var flverName = Path.GetFileNameWithoutExtension(flverVirtPath);
        var textureName = Path.GetFileName(texVirtPath);

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
                newModel.VirtualPath = flverVirtPath.ToLower();
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
                        newTexture.VirtualPath = texVirtPath.ToLower();
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

    public List<MapFlverInsightEntry> Models { get; set; } = new();

    public MapModelInsightEntry(string mapID, MapContainer container)
    {
        MapID = mapID;
        Container = container;
    }
}

public class MapFlverInsightEntry
{
    public string Name { get; set; }

    public string VirtualPath { get; set; }

    public FLVER2 FLVER2 { get; set; }

    public List<MapTextureInsightEntry> Entries { get; set; } = new();
}

public class MapTextureInsightEntry
{
    public string Name { get; set; }

    public string VirtualPath { get; set; }

    public string MaterialString { get; set; }

    public MTD MTD { get; set; }
    public MATBIN MATBIN { get; set; }
}
