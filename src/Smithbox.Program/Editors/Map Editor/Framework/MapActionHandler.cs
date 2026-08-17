using StudioCore.Utilities;

namespace StudioCore.Editors.MapEditor;

public class MapActionHandler
{
    private MapEditorView View;
    private ProjectEntry Project;

    public MapActionHandler(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Generate Navigation Data
    /// </summary>
    public void GenerateNavigationData()
    {
        HashSet<string> idCache = new();

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            string mapid = entry.Key.Filename;

            if (View.Project.Descriptor.ProjectType is ProjectType.DES)
            {
                if (mapid != "m03_01_00_99" && !mapid.StartsWith("m99"))
                {
                    var areaId = mapid.Substring(0, 3);
                    if (idCache.Contains(areaId))
                        continue;
                    idCache.Add(areaId);

                    var areaDirectories = new List<string>();
                    foreach (var tEntry in Project.Handler.MapData.PrimaryBank.Maps)
                    {
                        if (tEntry.Key.Filename.StartsWith(areaId) && tEntry.Key.Filename != "m03_01_00_99")
                        {
                            areaDirectories.Add(Path.Combine(View.Project.Descriptor.DataPath, "map", tEntry.Key.Filename));
                        }
                    }
                    SoulsMapMetadataGenerator.GenerateMCGMCP(View, areaDirectories, toBigEndian: true);
                }
                else
                {
                    var areaDirectories = new List<string> { Path.Combine(View.Project.Descriptor.DataPath, "map", mapid) };
                    SoulsMapMetadataGenerator.GenerateMCGMCP(View, areaDirectories, toBigEndian: true);
                }
            }
            else if (View.Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
            {
                var areaDirectories = new List<string> { Path.Combine(View.Project.Descriptor.DataPath, "map", mapid) };

                SoulsMapMetadataGenerator.GenerateMCGMCP(View, areaDirectories, toBigEndian: false);
            }
        }

        Smithbox.Log(this, LOC.Get("MAP_MCG_MCP_Log_Generated"));
    }

}
