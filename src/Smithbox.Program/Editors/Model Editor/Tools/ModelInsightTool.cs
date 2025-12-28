using SoulsFormats;
using StudioCore.Application;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public static class ModelInsightHelper
{
    public static ModelEditorScreen Editor;
    public static ProjectEntry Project;

    public static void Setup(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        Entries = new();
    }

    public static Dictionary<string, ModelInsightEntry> Entries { get; set; }
    public static ModelInsightEntry SelectedDataEntry { get; set; }
    public static FlverInsightEntry SelectedFlverEntry { get; set; }

    public static void AddEntry(ModelContainer container)
    {
        if (!Entries.ContainsKey(container.Name))
        {
            Entries.Add(container.Name, new ModelInsightEntry(container.Name, container));
        }
    }

    public static void ClearEntry(ModelContainer container)
    {
        if (Entries.ContainsKey(container.Name))
        {
            Entries.Remove(container.Name);
        }
    }

    public static void UpdateEntry(string flverVirtPath, string texVirtPath, IFlver flver, MTD mtd, MATBIN matbin, string materialStr)
    {
        if (Project == null)
            return;

        if (Project.ModelEditor == null)
            return;

        var flverName = Path.GetFileNameWithoutExtension(flverVirtPath);
        var textureName = Path.GetFileName(texVirtPath);

        if (Editor.Selection.SelectedModelWrapper == null)
            return;

        var wrapperName = Editor.Selection.SelectedModelWrapper.Name;

        if (Entries.ContainsKey(wrapperName))
        {
            var entry = Entries[wrapperName];

            if (!entry.Models.Any(e => e.Name == flverName))
            {
                var newModel = new FlverInsightEntry();
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
                        var newTexture = new TextureInsightEntry();
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

public class ModelInsightEntry
{
    public string MapID { get; set; }
    public ModelContainer Container { get; set; }

    public List<FlverInsightEntry> Models { get; set; } = new();

    public ModelInsightEntry(string mapID, ModelContainer container)
    {
        MapID = mapID;
        Container = container;
    }
}

public class FlverInsightEntry
{
    public string Name { get; set; }

    public string VirtualPath { get; set; }

    public FLVER2 FLVER2 { get; set; }

    public List<TextureInsightEntry> Entries { get; set; } = new();
}

public class TextureInsightEntry
{
    public string Name { get; set; }

    public string VirtualPath { get; set; }

    public string MaterialString { get; set; }

    public MTD MTD { get; set; }
    public MATBIN MATBIN { get; set; }
}
