using StudioCore;
using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.Editors.MapEditorNS;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Smithbox.Core.MapEditorNS;

public class MapData
{
    public BaseEditor BaseEditor;

    public Project Project;

    public MapBank PrimaryBank;

    public MsbMeta Meta;

    public FileDictionary MapFiles;
    public FileDictionary BtabFiles;

    /// <summary>
    /// JSON stores for this editor
    /// </summary>
    public EntitySelectionGroupList EntitySelections;

    public MapData(BaseEditor baseEditor, Project projectOwner)
    {
        BaseEditor = baseEditor;
        Project = projectOwner;

        PrimaryBank = new(this, "Primary");

        Meta = new(this);
        Meta.Load();

        MapFiles = new();
        MapFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "msb").ToList();

        BtabFiles = new();
        BtabFiles.Entries = Project.FileDictionary.Entries.Where(e => e.Extension == "btab").ToList();

        EntitySelections = new();
        LoadEntitySelectionGroups();
    }

    /// <summary>
    /// For saving the Entity Selection Groups.json on changes
    /// </summary>
    public void SaveEntitySelectionGroups()
    {
        var folder = $@"{Project.ProjectPath}\{ProjectUtils.DataFolder}\MSB\";
        var file = Path.Combine(folder, "Entity Selection Groups.json");

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(EntitySelections, SmithboxSerializerContext.Default.EntitySelectionGroupList);

        File.WriteAllText(file, json);
    }

    /// <summary>
    /// For loading the Entity Selection Groups.json on project setup
    /// </summary>
    public void LoadEntitySelectionGroups()
    {
        var folder = $@"{Project.ProjectPath}\{ProjectUtils.DataFolder}\MSB\";
        var file = Path.Combine(folder, "Entity Selection Groups.json");

        if(!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        if (!File.Exists(file))
        {
            EntitySelections = new();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var options = new JsonSerializerOptions();
                EntitySelections = JsonSerializer.Deserialize(filestring, SmithboxSerializerContext.Default.EntitySelectionGroupList);

                if (EntitySelections == null)
                {
                    throw new Exception("[Smithbox:Map Editor] Failed to read Entity Selection Groups.json");
                }
            }
            catch (Exception e)
            {
                TaskLogs.AddLog("[Smithbox] Failed to load Entity Selection Groups.json");

                EntitySelections = new EntitySelectionGroupList();
            }
        }
    }
}
