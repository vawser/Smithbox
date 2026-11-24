using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The fall protection (f) collision the Create tab makes alongside the high and low collision: flat vertical walls
/// along the outer edges of the new mesh, optionally only those facing one direction or with a drop past them. The
/// walls go in the model's f file (made from another of the binder's files if the model has none), and the f binder is
/// written into the project. ER, NR and DB_NT hold each wall run as a body of its own (Havok 2018); AC6 holds them all
/// as the instances of one compound body (Havok 2019, CollisionEditorFallShapesAc6). The other games have none.
/// </summary>
public class CollisionEditorFallProtection
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly CollisionEditorTool Tool;

    private const float MinHeight = 0.01f;

    public CollisionEditorFallProtection(MapEditorView view, ProjectEntry project, CollisionEditorTool tool)
    {
        View = view;
        Project = project;
        Tool = tool;
    }

    /// <summary>
    /// The games whose f files the walls are built for: ER, NR and DB_NT (Havok 2018 extern mesh bodies) and AC6
    /// (Havok 2019 compound bodies).
    /// </summary>
    public bool IsSupported => IsAc6 || Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB_NT;

    private bool IsAc6 => Project.Descriptor.ProjectType is ProjectType.AC6;

    /// <summary>
    /// The wall height, kept per layout: AC6's walls are 10 high, ER's, NR's and DB_NT's 1.8, so each starts at its
    /// game's.
    /// </summary>
    private ref float Height => ref IsAc6
        ? ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Height_AC6
        : ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Height;

    /// <summary>The bodies of an f file's physics system, of the game's Havok version. Null if it has none.</summary>
    public System.Collections.IList GetBodies(Havok.HavokDocument document)
    {
        return IsAc6
            ? CollisionEditorFallShapesAc6.GetSystem(document)?.m_bodyCinfos
            : CollisionEditorFallShapes.GetSystem(document)?.m_bodyCinfos;
    }

    private bool IsTemplate(Havok.HavokDocument document)
    {
        return IsAc6 ? CollisionEditorFallShapesAc6.IsTemplate(document) : CollisionEditorFallShapes.IsTemplate(document);
    }

    /// <summary>The bodies of the walls: one per wall run (hk2018), or one compound of them all (AC6).</summary>
    private List<object> MakeBodies(CollisionEditorFallShapes.WallResult walls, uint filter)
    {
        if (IsAc6)
            return new List<object> { CollisionEditorFallShapesAc6.MakeBody(walls.Bodies, filter) };

        return walls.Bodies.Select(b => (object)CollisionEditorFallShapes.MakeBody(b, filter, MotionPropertiesId)).ToList();
    }

    /// <summary>
    /// The motion properties of the game's fall protection bodies: DB_NT's are all 0, ER's and NR's 0xFFFF (none).
    /// </summary>
    private ushort MotionPropertiesId => Project.Descriptor.ProjectType is ProjectType.DB_NT
        ? (ushort)0
        : CollisionEditorFallShapes.NoMotionProperties;

    #region Display

    /// <summary>The wall options: direction, drop edges, height and collision filter.</summary>
    public void DisplayOptions()
    {
        // Direction
        var direction = CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Direction;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionFallDirection", LOC.Get(direction.GetDisplayName())))
        {
            foreach (var value in Enum.GetValues<FallProtectionDirection>())
            {
                if (ImGui.Selectable($"{LOC.Get(value.GetDisplayName())}##collisionFallDirection{value}", value == direction))
                    CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Direction = value;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip($"{LOC.Get("MAP_CollisionEditor_Fall_Direction_Header_TT")}\n\n{LOC.Get("MAP_CollisionEditor_Fall_Direction_Input_TT")}");

        // Drop Edges
        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Fall_Drop_Only")}##collisionFallDropOnly", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Drop_Only);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Drop_Only_TT"));

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Drop_Only)
        {
            if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Fall_Min_Drop")}##collisionFallMinDrop", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Min_Drop, 0.1f, 1.0f, "%.2f"))
                CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Min_Drop = MathF.Max(0f, CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Min_Drop);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Min_Drop_TT"));
        }

        // Wall Height
        if (ImGui.InputFloat($"{LOC.Get("MAP_CollisionEditor_Fall_Height_Header")}##collisionFallHeight", ref Height, 0.1f, 1.0f, "%.2f"))
            Height = MathF.Max(MinHeight, Height);
        GUI.Tooltip($"{LOC.Get("MAP_CollisionEditor_Fall_Height_Header_TT")}\n\n{LOC.Get("MAP_CollisionEditor_Fall_Height_Input_TT")}");

        // Collision Filter
        if (ImGui.InputInt($"{LOC.Get("MAP_CollisionEditor_Fall_Filter_Header")}##collisionFallFilter", ref CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Filter))
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Filter = Math.Max(0, CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Filter);
        GUI.Tooltip($"{LOC.Get("MAP_CollisionEditor_Fall_Filter_Header_TT")}\n\n{LOC.Get("MAP_CollisionEditor_Fall_Filter_Input_TT")}");
    }

    #endregion

    #region Walls

    /// <summary>
    /// The walls along the outer edges of a mesh (in the space of the part), placed in the world by the part's world
    /// matrix. With Drop Edges Only, the mesh itself and the loaded collision parts around it (other than those placing
    /// the model) are the ground.
    /// </summary>
    public CollisionEditorFallShapes.WallResult BuildWalls(List<Vector3> vertices, List<int> indices, Matrix4x4 world, string modelName)
    {
        var triangles = new List<List<Vector3>> { indices.Select(i => vertices[i]).ToList() };

        CollisionEditorFallShapes.DropFilter drop = null;

        if (CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Drop_Only)
        {
            var minDrop = CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Min_Drop;
            var ground = new FallGround();
            ground.Add(triangles, world);

            foreach (var (entity, fileName) in GetGroundNeighbours(modelName, GetWorldBounds(triangles, world), minDrop))
            {
                var neighbour = View.HavokCollisionBank.GetEditableDocument(fileName);

                if (neighbour != null)
                    ground.Add(Havok.Shared.CollisionGeometry.Submeshes(neighbour.Objects), entity.GetWorldMatrix());
            }

            drop = new CollisionEditorFallShapes.DropFilter(ground, minDrop);
        }

        return CollisionEditorFallShapes.BuildWalls(triangles, world,
            CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Direction, Height, drop);
    }

    private static (Vector3 Min, Vector3 Max) GetWorldBounds(List<List<Vector3>> triangleLists, Matrix4x4 world)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);

        foreach (var corners in triangleLists)
        {
            foreach (var corner in corners)
            {
                var p = Vector3.Transform(corner, world);
                min = Vector3.Min(min, p);
                max = Vector3.Max(max, p);
            }
        }

        return (min, max);
    }

    /// <summary>
    /// The other collision parts of the loaded maps whose drawn collision reaches the ground just past the mesh's
    /// edges, with their high collision file: ground there is what can be landed on. Ground further below than the
    /// minimum drop is a drop either way, so it isn't needed.
    /// </summary>
    private List<(MsbEntity Entity, string FileName)> GetGroundNeighbours(string modelName, (Vector3 Min, Vector3 Max) bounds, float minDrop)
    {
        var reach = CollisionEditorFallShapes.ProbeDistance + 0.5f;
        var min = bounds.Min - new Vector3(reach, minDrop + reach, reach);
        var max = bounds.Max + new Vector3(reach, CollisionEditorFallShapes.ProbeRise + reach, reach);

        var bank = View.HavokCollisionBank;
        var result = new List<(MsbEntity, string)>();

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
                continue;

            foreach (var ent in entry.Value.MapContainer.Objects)
            {
                if (ent is not MsbEntity entity || !EntityHelper.IsPartCollision(entity) || !entity.EditorVisible)
                    continue;

                // Only parts whose drawn mesh has loaded, and overlaps, are looked at
                if (entity.RenderSceneMesh is not { } mesh)
                    continue;

                var b = mesh.GetBounds();

                if (b.Max.X < min.X || b.Min.X > max.X || b.Max.Y < min.Y || b.Min.Y > max.Y || b.Max.Z < min.Z || b.Min.Z > max.Z)
                    continue;

                if (entity.GetPropertyValue("ModelName") is not string model || model.Length != 7)
                    continue;

                // A part placing the same model is the collision itself (an overwrite's parts)
                if (string.Equals(model, modelName, StringComparison.OrdinalIgnoreCase))
                    continue;

                foreach (var letter in new[] { "h", "l" })
                {
                    var fileName = GetFileName(letter, entity.MapID, model.Substring(1));

                    if (bank.HasMapCollision(entity.MapID, fileName))
                    {
                        result.Add((entity, fileName));
                        break;
                    }
                }
            }
        }

        return result;
    }

    public static string GetFileName(string type, string mapId, string number) => $"{type}{mapId.Substring(1)}_{number}.hkx.dcx";

    #endregion

    #region Write

    /// <summary>
    /// Puts the walls in the model's f file, replacing any walls it had. A missing file is made from another file of
    /// the binder, with the walls as its bodies; an existing file's bodies are replaced as one undoable action. Then
    /// the f binder is written. Returns why it failed, or null. Created is whether the file is new, so its mesh has to
    /// be loaded for the parts already placing the model.
    /// </summary>
    public string WriteWalls(MapContainer map, string number, CollisionEditorFallShapes.WallResult walls, out string fileName, out bool created)
    {
        var bank = View.HavokCollisionBank;
        var mapId = map.Name;

        fileName = GetFileName("f", mapId, number);
        created = false;

        List<object> bodies;

        try
        {
            bodies = MakeBodies(walls, (uint)CFG.Current.MapEditor.MapEditor_CollisionEditor_Fall_Filter);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Fall_Build_Failed", fileName), e);
            return LOC.Get("MAP_CollisionEditor_Fall_Build_Failed", fileName);
        }

        if (!bank.HasMapCollision(mapId, fileName))
        {
            var template = bank.ReadTemplateDocument(mapId, "f", IsTemplate);
            var templateBodies = GetBodies(template);

            if (templateBodies == null)
                return LOC.Get("MAP_CollisionEditor_Fall_No_Template", $"f{mapId.Substring(1)}");

            // A new file, as the h and l files Create makes: written with its walls, not undone
            templateBodies.Clear();

            foreach (var body in bodies)
                templateBodies.Add(body);

            bank.AddMapCollision(mapId, "f", fileName, template);
            Tool.OnCollisionCreated();
            created = true;
        }
        else
        {
            var existing = GetBodies(bank.GetEditableDocument(fileName));

            if (existing == null)
                return LOC.Get("MAP_CollisionEditor_Fall_Unsupported_File", fileName);

            View.ViewportActionManager.ExecuteAction(new FallProtectionAction(map, fileName, existing, bodies,
                name => Tool.OnBodiesChanged(mapId, name)));
        }

        if (!bank.SaveMapCollision(mapId, "f"))
            return LOC.Get("MAP_CollisionEditor_Fall_Save_Failed", fileName);

        return null;
    }

    /// <summary>
    /// Loads a new f file's mesh, by name, as Create loads a new collision's, then gives the parts drawing it new
    /// meshes so they pick it up.
    /// </summary>
    public void LoadCollision(MapContainer map, string modelName, string fileName)
    {
        if (!View.HavokCollisionBank.IsCollisionTypeVisible(HavokCollisionType.FallProtection))
            return;

        var assetMapId = PathBuilder.GetAssetMapID(Project, map.Name);
        var assetName = ModelLocator.MapModelNameToAssetName(Project, assetMapId, modelName);
        var asset = ModelLocator.GetMapCollisionModel(Project, assetMapId, assetName, false, HavokCollisionType.FallProtection);

        if (!asset.IsValid())
            return;

        ResourceManager.DefaultPool.LoadAsset<HavokCollisionResource>(Project, asset, new LoadOptions
        {
            JobName = LOC.Get("MAP_ResourceManager_Collision_Models"),
            Types = ResourceType.CollisionHKX,
            AccessLevel = AccessLevel.AccessGPUOptimizedOnly,
            OnlyNames = new[] { asset.Name }
        });

        Tool.OnBodiesChanged(map.Name, fileName);
    }

    #endregion
}
