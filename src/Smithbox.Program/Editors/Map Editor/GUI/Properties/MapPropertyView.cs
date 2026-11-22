using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using StudioCore.Utilities;

namespace StudioCore.Editors.MapEditor;

public class MapPropertyView
{
    private MapEditorView View;
    private ProjectEntry Project;

    public MapMsbPropertyView MsbPropertyView;

    public string MapPropFilter = "";
    public bool ExactMapPropFilter = false;

    public MapPropertyView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        MsbPropertyView = new(view, project);
    }

    public void Display()
    {
        DisplayCommonHeader();

        // A collision part's collision file and a navmesh entry's navmesh file are edited in the Havok Editor
        // (the header's link button)
        MsbPropertyView.Display();
    }

    public void DisplayCommonHeader()
    {
        ImGui.BeginChild($"framedList_MapProperties", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("MapPropSearch", ref MapPropFilter, ref ExactMapPropFilter);

        // Action: Go To
        DisplayGoToButton();

        // Action: Open in Havok Editor
        if (IsCollisionType())
        {
            DisplayHavokEditorButton(HavokCategoryMode.Map_Collision, GetHavokCollisionTarget(),
                "MAP_Properties_Havok_Editor_TT", "MAP_Properties_Havok_Editor_Unavailable_TT");
        }
        else if (IsNavmeshType())
        {
            DisplayHavokEditorButton(HavokCategoryMode.Navmesh, GetHavokNavmeshTarget(),
                "MAP_Properties_Havok_Editor_Navmesh_TT", "MAP_Properties_Havok_Editor_Navmesh_Unavailable_TT");
        }

        // Toggle: Community Names
        GUI.DisplayToggleButton("communityNameToggle", Icons.Book,
            ref CFG.Current.MapEditor_Properties_Enable_Commmunity_Names,
            "MAP_Properties_Toggle_Community_Names_Internal",
            "MAP_Properties_Toggle_Community_Names_Community",
            "MAP_Properties_Toggle_Community_Names_TT");

        DisplayMsbHeaderToggles();

        ImGui.EndChild();
    }

    /// <summary>
    /// Shows the selected entity (the first, with several selected) in the Map Contents tree
    /// </summary>
    private void DisplayGoToButton()
    {
        var target = GetGoToTarget();

        ImGui.SameLine();

        ImGui.BeginDisabled(target == null);

        if (ImGui.Button($"{Icons.LocationArrow}##propertiesGoTo", DPI.IconButtonSize))
        {
            View.MapContentView.RevealEntity(target);
        }

        ImGui.EndDisabled();

        GUI.Tooltip(LOC.Get("MAP_Properties_Go_To_TT"));
    }

    /// <summary>
    /// The first selected entity that belongs to a loaded map, as the selection orders it
    /// </summary>
    private Entity GetGoToTarget()
    {
        if (!View.Universe.HasProcessedMapLoad)
            return null;

        foreach (var ent in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            if (ent.Container is MapContainer { LoadState: MapContentLoadState.Loaded })
                return ent;
        }

        return null;
    }

    /// <summary>
    /// Shows a file of the selected entity (its collision or navmesh file) in the Havok Editor
    /// </summary>
    private void DisplayHavokEditorButton(HavokCategoryMode category, (string Binder, string File)? target,
        string tooltipKey, string unavailableKey)
    {
        var available = target != null
            && Project.Descriptor.EnableHavokEditor
            && EditorConditions.SupportsHavokEditor(Project.Descriptor.ProjectType)
            && Project.Handler.HavokEditor != null;

        ImGui.SameLine();

        ImGui.BeginDisabled(!available);

        if (ImGui.Button($"{Icons.ExternalLink}##propertiesHavokEditor", DPI.IconButtonSize))
        {
            EditorCommandQueue.AddCommand($"hkx/select/{category}/{target.Value.Binder}/{target.Value.File}");
        }

        ImGui.EndDisabled();

        GUI.Tooltip(available
            ? LOC.Get(tooltipKey, target.Value.File)
            : LOC.Get(unavailableKey));
    }

    /// <summary>
    /// The binder and file name of the selected collision part's collision (h30_00_00_00, h30_00_00_00_000100.hkx.dcx):
    /// the first displayed collision type the map has a file of, else the first type it has one of. Null if the part
    /// has no model name or its map has no such file.
    /// </summary>
    private (string Binder, string File)? GetHavokCollisionTarget()
    {
        var ent = View.ViewportSelection.GetFilteredSelection<Entity>().FirstOrDefault();

        if (ent?.WrappedObject == null || ent.Container is not MapContainer map)
            return null;

        var modelName = ent.WrappedObject.GetType().GetProperty("ModelName")?.GetValue(ent.WrappedObject) as string;

        if (string.IsNullOrEmpty(modelName))
            return null;

        var mapId = map.Name;
        var bank = View.HavokCollisionBank;

        (string, string) MakeTarget(HavokCollisionType type)
        {
            var prefix = type switch
            {
                HavokCollisionType.Low => "l",
                HavokCollisionType.FallProtection => "f",
                _ => "h"
            };

            var binder = $"{prefix}{mapId.Replace("m", "")}";
            return (binder, $"{binder}_{modelName.Replace("h", "")}.hkx.dcx");
        }

        var types = bank.GetVisibleCollisionTypes().Concat(bank.GetSupportedCollisionTypes()).Distinct().ToList();

        // The map's collision isn't listed when collision loading is off: the Havok Editor reports a missing file
        if (bank.GetMapCollisionNames(mapId).Count == 0)
            return MakeTarget(types[0]);

        foreach (var type in types)
        {
            var target = MakeTarget(type);

            if (bank.HasMapCollision(mapId, target.Item2))
                return target;
        }

        return null;
    }

    /// <summary>
    /// The binder and file name of the selected navmesh entry's navmesh (m30_00_00_00, n30_00_00_00_000100): the file
    /// its faces are in. In ER, NR and DB_NT that is its own set's file (n or o) or its quarter's q file, named as the
    /// Map Editor's navmesh drawing names it; in AC6 and DB its model's n file. Null for an entry that isn't a navmesh
    /// entry (e.g. the entity holding the whole NVA), or whose file the map's binder doesn't have.
    /// </summary>
    private (string Binder, string File)? GetHavokNavmeshTarget()
    {
        var ent = View.ViewportSelection.GetFilteredSelection<Entity>().FirstOrDefault();

        if (ent?.WrappedObject is not NVA.Navmesh entry || ent.Container is not MapContainer map)
            return null;

        var mapName = map.Name;
        var fileNames = View.HavokNavmeshBank.GetBinderFileNames(mapName);

        string fileName;

        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB_NT)
        {
            // NR names an open world tile version's files from the binder's names (its NameIDs have no _DD); ER from the NameID
            var profile = NavGameProfile.For(Project.Descriptor.ProjectType.ToString());
            fileName = NavigationMap.FileNameFromEntry(mapName, entry, profile, profile.NameIdNamesTileVersion ? null : fileNames);
        }
        else
        {
            fileName = NavigationMap.MeshName(mapName, NavmeshSet.N, entry.ModelID);
        }

        if (!fileNames.Contains(fileName, StringComparer.OrdinalIgnoreCase))
            return null;

        return (mapName, fileName);
    }

    public void DisplayMsbHeaderToggles()
    {
        // Toggle: Display Unknown
        GUI.DisplayToggleButton("displayUnkToggle", Icons.Eye,
            ref CFG.Current.MapEditor_Properties_Display_Unknown_Properties,
            "MAP_Properties_Toggle_Unknown_Field_Hide",
            "MAP_Properties_Toggle_Unknown_Field_Show",
            "MAP_Properties_Toggle_Unknown_Field_TT");

        // Toggle: Display Padding
        GUI.DisplayToggleButton("displayPaddingToggle", Icons.Hubzilla,
            ref CFG.Current.MapEditor_Field_List_Display_Padding,
            "MAP_Properties_Toggle_Padding_Hide",
            "MAP_Properties_Toggle_Padding_Show",
            "MAP_Properties_Toggle_Padding_TT");

        // Toggle: Display References
        GUI.DisplayToggleButton("displayReferencesToggle", Icons.Search,
            ref CFG.Current.MapEditor_Field_List_Display_References,
            "MAP_Properties_Toggle_References_Hide",
            "MAP_Properties_Toggle_References_Show",
            "MAP_Properties_Toggle_References_TT");
    }

    public bool IsCollisionType()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        if (View.Universe.HasProcessedMapLoad && entSelection.Count > 0)
        {
            Entity firstEnt = entSelection.First();

            // Is Collision Map Object
            if (Project.Descriptor.ProjectType is ProjectType.ER
                or ProjectType.AC6
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                if (EntityHelper.IsPartCollision(firstEnt))
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool IsNavmeshType()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        if (View.Universe.HasProcessedMapLoad && entSelection.Count > 0)
        {
            Entity firstEnt = entSelection.First();

            // Is Navmesh Map Object
            if (Project.Descriptor.ProjectType is ProjectType.ER
                or ProjectType.AC6
                or ProjectType.NR
                or ProjectType.DB_NT
                or ProjectType.DB)
            {
                if (EntityHelper.IsNavmesh(firstEnt))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
