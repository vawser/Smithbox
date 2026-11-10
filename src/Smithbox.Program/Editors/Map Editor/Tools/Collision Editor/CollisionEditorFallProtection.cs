using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Fall Protection tab of the Collision Editor: generates the fall protection (f) collision of the selected
/// collision part, as flat vertical walls along the outer edges of its high or low collision, optionally only those
/// facing one direction. The walls are previewed in the viewport, then put in the part's f file (made from another of
/// the binder's files if the part has none) as one undoable action, and the f binder is written into the project.
/// ER and NR only: AC6's f files are laid out differently (compound shapes), and the other games have none.
/// </summary>
public class CollisionEditorFallProtection
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly CollisionEditorTool Tool;

    // The walls of the inputs as of the last time they changed, and what they were built from
    private CollisionEditorFallShapes.WallResult Walls;
    private string WallsKey;
    private string SourceProblem;

    // The source file's triangles and world bounds, as of the last time it or the part changed
    private string SourceKey;
    private List<List<Vector3>> SourceTriangles;
    private (Vector3 Min, Vector3 Max)? SourceBounds;

    // How many other parts' collision the drop filter took as ground
    private int GroundParts;

    // The selected part and its files, as of this frame
    private MsbEntity Part;
    private string ModelNumber;
    private string FallFileName;

    // The walls drawn in the viewport while the tab is open
    private MeshRenderableProxy PreviewMesh;
    private string PreviewKey;
    private int LastDisplayFrame = -10;

    private const float MinHeight = 0.01f;

    public CollisionEditorFallProtection(MapEditorView view, ProjectEntry project, CollisionEditorTool tool)
    {
        View = view;
        Project = project;
        Tool = tool;
    }

    /// <summary>The games whose f files the walls are built for: ER, NR and DB_NT (Havok 2018 extern meshes).</summary>
    public bool IsSupported => Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.DB_NT;

    private bool CanUse => IsSupported;

    /// <summary>
    /// The motion properties of the game's fall protection bodies: DB_NT's are all 0, ER's and NR's 0xFFFF (none).
    /// </summary>
    private ushort MotionPropertiesId => Project.Descriptor.ProjectType is ProjectType.DB_NT
        ? (ushort)0
        : CollisionEditorFallShapes.NoMotionProperties;

    #region Display

    public void Display()
    {
        LastDisplayFrame = Tool.CurrentFrame;

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_Description"));

        if (!CanUse)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_Unsupported_Game"));
            Clear();
            return;
        }

        // Source
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Part_Header"), LOC.Get("MAP_CollisionEditor_Fall_Part_Header_TT"));

        UpdatePart();

        if (Part == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_No_Part"));
            DisplayPartPicker();
            Clear();

            // The last Generate's result, so it is still seen once the part is no longer selected
            Tool.DisplayLastResult(CollisionEditorSource.Fall);
            return;
        }

        var bank = View.HavokCollisionBank;
        var mapId = Part.MapID;

        GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_Part", Part.Name, $"h{ModelNumber}", mapId));

        if (!bank.HasCollisionBinder(mapId, "f"))
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_No_Binder", mapId));
            Clear();
            return;
        }

        // Source Layer
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Source_Header"), LOC.Get("MAP_CollisionEditor_Fall_Source_Header_TT"));

        var source = CFG.Current.MapEditor_CollisionEditor_Fall_Source;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionFallSource", LOC.Get(source.GetDisplayName())))
        {
            foreach (var type in new[] { HavokCollisionType.High, HavokCollisionType.Low })
            {
                if (ImGui.Selectable($"{LOC.Get(type.GetDisplayName())}##collisionFallSource{type}", type == source))
                    CFG.Current.MapEditor_CollisionEditor_Fall_Source = type;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Source_Input_TT"));

        // Direction
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Direction_Header"), LOC.Get("MAP_CollisionEditor_Fall_Direction_Header_TT"));

        var direction = CFG.Current.MapEditor_CollisionEditor_Fall_Direction;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionFallDirection", LOC.Get(direction.GetDisplayName())))
        {
            foreach (var value in Enum.GetValues<FallProtectionDirection>())
            {
                if (ImGui.Selectable($"{LOC.Get(value.GetDisplayName())}##collisionFallDirection{value}", value == direction))
                    CFG.Current.MapEditor_CollisionEditor_Fall_Direction = value;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Direction_Input_TT"));

        // Drop Edges
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Drop_Header"), LOC.Get("MAP_CollisionEditor_Fall_Drop_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Fall_Drop_Only")}##collisionFallDropOnly", ref CFG.Current.MapEditor_CollisionEditor_Fall_Drop_Only);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Drop_Only_TT"));

        if (CFG.Current.MapEditor_CollisionEditor_Fall_Drop_Only)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.InputFloat("##collisionFallMinDrop", ref CFG.Current.MapEditor_CollisionEditor_Fall_Min_Drop, 0.1f, 1.0f, "%.2f"))
                CFG.Current.MapEditor_CollisionEditor_Fall_Min_Drop = MathF.Max(0f, CFG.Current.MapEditor_CollisionEditor_Fall_Min_Drop);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Min_Drop_TT"));
        }

        // Wall Height
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Height_Header"), LOC.Get("MAP_CollisionEditor_Fall_Height_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputFloat("##collisionFallHeight", ref CFG.Current.MapEditor_CollisionEditor_Fall_Height, 0.1f, 1.0f, "%.2f"))
            CFG.Current.MapEditor_CollisionEditor_Fall_Height = MathF.Max(MinHeight, CFG.Current.MapEditor_CollisionEditor_Fall_Height);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Height_Input_TT"));

        // Collision Filter
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Filter_Header"), LOC.Get("MAP_CollisionEditor_Fall_Filter_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        if (ImGui.InputInt("##collisionFallFilter", ref CFG.Current.MapEditor_CollisionEditor_Fall_Filter))
            CFG.Current.MapEditor_CollisionEditor_Fall_Filter = Math.Max(0, CFG.Current.MapEditor_CollisionEditor_Fall_Filter);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Filter_Input_TT"));

        // Existing Walls
        var existing = GetExistingBodyCount();

        if (existing > 0)
        {
            GUI.Spacer();
            GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Existing_Header"), LOC.Get("MAP_CollisionEditor_Fall_Existing_Header_TT"));

            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_Existing", FallFileName, existing));

            ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Fall_Keep_Existing")}##collisionFallKeep", ref CFG.Current.MapEditor_CollisionEditor_Fall_Keep_Existing);
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Keep_Existing_TT"));
        }

        UpdateWalls();

        // Preview
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Preview_Header"), LOC.Get("MAP_CollisionEditor_Fall_Preview_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Fall_Show_Preview")}##collisionFallShowPreview", ref CFG.Current.MapEditor_CollisionEditor_Fall_Show_Preview);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Show_Preview_TT"));

        if (SourceProblem != null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, SourceProblem);
        }
        else if (Walls != null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_Summary", Walls.OuterEdges, Walls.FacingEdges,
                Walls.TriangleCount / 2, Walls.Bodies.Count));

            if (CFG.Current.MapEditor_CollisionEditor_Fall_Drop_Only)
                GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Fall_Drop_Summary", Walls.KeptEdges, GroundParts));

            if (Walls.Bodies.Count == 0)
                GUI.WrappedText(LOC.Get(CFG.Current.MapEditor_CollisionEditor_Fall_Drop_Only
                    ? "MAP_CollisionEditor_Fall_No_Drop_Walls"
                    : "MAP_CollisionEditor_Fall_No_Walls"));
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Fall_Actions_Header"), LOC.Get("MAP_CollisionEditor_Fall_Actions_Header_TT"));

        var canGenerate = SourceProblem == null && Walls is { Bodies.Count: > 0 };

        GUI.ConditionalMultiButtonInput("collisionFallActions",
            "collisionFallGenerate", LOC.Get("MAP_CollisionEditor_Fall_Generate"), LOC.Get("MAP_CollisionEditor_Fall_Generate_TT"),
            Generate, canGenerate);

        Tool.DisplayLastResult(CollisionEditorSource.Fall);
    }

    /// <summary>
    /// The collision parts of the active map, to select one without finding it in the viewport or the content tree
    /// (collision is often hidden there, and h and l draw the same part twice).
    /// </summary>
    private void DisplayPartPicker()
    {
        var map = View.Selection.ResolveTargetMap(MapTargetPicker.ActiveMap);

        if (map == null)
            return;

        var parts = map.Objects.OfType<MsbEntity>().Where(EntityHelper.IsPartCollision).ToList();

        if (parts.Count == 0)
            return;

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo("##collisionFallPartPicker", LOC.Get("MAP_CollisionEditor_Fall_Pick_Part", map.Name)))
        {
            foreach (var part in parts)
            {
                var model = part.GetPropertyValue("ModelName") as string;

                if (ImGui.Selectable($"{part.Name} ({model})##collisionFallPick{part.Name}"))
                    Tool.SelectAndFrame(part);
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Fall_Pick_Part_TT"));
    }

    /// <summary>
    /// The single selected collision part, and the names of its files. Null if the selection isn't one.
    /// </summary>
    private void UpdatePart()
    {
        var selection = View.ViewportSelection.GetFilteredSelection<MsbEntity>();
        var entity = selection.Count == 1 ? selection.First() : null;

        Part = null;
        ModelNumber = null;
        FallFileName = null;

        if (entity == null || !EntityHelper.IsPartCollision(entity) || entity.Container is not MapContainer)
            return;

        // h001234: the number is the files' last part, f10_00_00_00_001234.hkx.dcx
        if (entity.GetPropertyValue("ModelName") is not string model || model.Length != 7 || !model.Substring(1).All(char.IsDigit))
            return;

        Part = entity;
        ModelNumber = model.Substring(1);
        FallFileName = GetFileName("f", entity.MapID, ModelNumber);
    }

    private static string GetFileName(string type, string mapId, string number) => $"{type}{mapId.Substring(1)}_{number}.hkx.dcx";

    /// <summary>The bodies the part's f file has now, without reading its binder if it hasn't been read.</summary>
    private int GetExistingBodyCount()
    {
        if (!View.HavokCollisionBank.HasMapCollision(Part.MapID, FallFileName))
            return 0;

        var system = CollisionEditorFallShapes.GetSystem(View.HavokCollisionBank.GetEditableDocument(FallFileName));
        return system?.m_bodyCinfos.Count ?? 0;
    }

    /// <summary>
    /// Builds the walls again when an input they depend on has changed, including the part being moved, as the
    /// direction is the world's.
    /// </summary>
    private void UpdateWalls()
    {
        var sourceType = CFG.Current.MapEditor_CollisionEditor_Fall_Source;
        var sourceName = GetFileName(sourceType is HavokCollisionType.Low ? "l" : "h", Part.MapID, ModelNumber);
        var world = Part.GetWorldMatrix();
        var bank = View.HavokCollisionBank;

        var document = bank.HasMapCollision(Part.MapID, sourceName) ? bank.GetEditableDocument(sourceName) : null;

        // A commit in the Edit tab gives the bodies new shapes in the same document
        var shapes = document == null
            ? ""
            : string.Join(",", Havok.Shared.CollisionGeometry.Bodies(document.Objects).Select(b => b.Shape?.GetHashCode()));

        var sourceKey = $"{sourceName}|{document?.GetHashCode()}|{shapes}|{world}";

        if (sourceKey != SourceKey)
        {
            SourceKey = sourceKey;
            SourceTriangles = null;
            SourceBounds = null;

            if (document != null)
            {
                try
                {
                    SourceTriangles = Havok.Shared.CollisionGeometry.Submeshes(document.Objects);
                    SourceBounds = GetWorldBounds(SourceTriangles, world);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Fall_Source_Failed", sourceName), e);
                }
            }
        }

        var dropOnly = CFG.Current.MapEditor_CollisionEditor_Fall_Drop_Only;
        var minDrop = CFG.Current.MapEditor_CollisionEditor_Fall_Min_Drop;
        var neighbours = dropOnly && SourceBounds != null
            ? GetGroundNeighbours(sourceType, SourceBounds.Value, minDrop)
            : new List<(MsbEntity Entity, string FileName)>();

        var key = $"{sourceKey}|{CFG.Current.MapEditor_CollisionEditor_Fall_Direction}|{CFG.Current.MapEditor_CollisionEditor_Fall_Height}|" +
            $"{dropOnly}|{minDrop}|{string.Join(",", neighbours.Select(n => $"{n.FileName}@{n.Entity.GetWorldMatrix()}"))}";

        if (key == WallsKey)
            return;

        WallsKey = key;
        Walls = null;
        SourceProblem = null;
        GroundParts = neighbours.Count;

        if (document == null)
        {
            SourceProblem = LOC.Get("MAP_CollisionEditor_Fall_No_Source", sourceName);
            return;
        }

        if (SourceTriangles == null)
        {
            SourceProblem = LOC.Get("MAP_CollisionEditor_Fall_Source_Failed", sourceName);
            return;
        }

        try
        {
            CollisionEditorFallShapes.DropFilter drop = null;

            if (dropOnly)
            {
                // The part's own collision, and that of the parts around it, is the ground that can be landed on
                var ground = new FallGround();
                ground.Add(SourceTriangles, world);

                foreach (var (entity, fileName) in neighbours)
                {
                    var neighbour = bank.GetEditableDocument(fileName);

                    if (neighbour != null)
                        ground.Add(Havok.Shared.CollisionGeometry.Submeshes(neighbour.Objects), entity.GetWorldMatrix());
                }

                drop = new CollisionEditorFallShapes.DropFilter(ground, minDrop);
            }

            Walls = CollisionEditorFallShapes.BuildWalls(SourceTriangles, world,
                CFG.Current.MapEditor_CollisionEditor_Fall_Direction, CFG.Current.MapEditor_CollisionEditor_Fall_Height, drop);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Fall_Source_Failed", sourceName), e);
            SourceProblem = LOC.Get("MAP_CollisionEditor_Fall_Source_Failed", sourceName);
        }
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
    /// The other collision parts of the loaded maps whose drawn collision reaches the ground just past the part's
    /// edges, with the file of the same layer as the source: ground there is what can be landed on. Ground further
    /// below than the minimum drop is a drop either way, so it isn't needed.
    /// </summary>
    private List<(MsbEntity Entity, string FileName)> GetGroundNeighbours(HavokCollisionType layer, (Vector3 Min, Vector3 Max) bounds,
        float minDrop)
    {
        var reach = CollisionEditorFallShapes.ProbeDistance + 0.5f;
        var min = bounds.Min - new Vector3(reach, minDrop + reach, reach);
        var max = bounds.Max + new Vector3(reach, CollisionEditorFallShapes.ProbeRise + reach, reach);

        var letter = layer is HavokCollisionType.Low ? "l" : "h";
        var bank = View.HavokCollisionBank;
        var result = new List<(MsbEntity, string)>();

        foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
        {
            if (entry.Value.MapContainer == null)
                continue;

            foreach (var ent in entry.Value.MapContainer.Objects)
            {
                if (ent == Part || ent is not MsbEntity entity || !EntityHelper.IsPartCollision(entity) || !entity.EditorVisible)
                    continue;

                // Only parts whose drawn mesh has loaded, and overlaps, are looked at
                if (entity.RenderSceneMesh is not { } mesh)
                    continue;

                var b = mesh.GetBounds();

                if (b.Max.X < min.X || b.Min.X > max.X || b.Max.Y < min.Y || b.Min.Y > max.Y || b.Max.Z < min.Z || b.Min.Z > max.Z)
                    continue;

                if (entity.GetPropertyValue("ModelName") is not string model || model.Length != 7)
                    continue;

                var fileName = GetFileName(letter, entity.MapID, model.Substring(1));

                if (bank.HasMapCollision(entity.MapID, fileName))
                    result.Add((entity, fileName));
            }
        }

        return result;
    }

    /// <summary>Lets go of the walls and their preview, for when there is nothing to generate for.</summary>
    private void Clear()
    {
        Walls = null;
        WallsKey = null;
        SourceProblem = null;
        SourceKey = null;
        SourceTriangles = null;
        SourceBounds = null;
        DisposePreview();
    }

    #endregion

    #region Preview

    /// <summary>
    /// Draws the walls in the viewport, placed with the part, while the tab is open. Called each frame by the tool.
    /// </summary>
    public void UpdatePreview(RenderScene scene)
    {
        // The tab is drawn after the viewport, so it shows as open if it was drawn last frame
        var shown = Tool.CurrentFrame - LastDisplayFrame <= 1
            && CFG.Current.MapEditor_CollisionEditor_Fall_Show_Preview
            && Part != null
            && Walls is { Bodies.Count: > 0 };

        if (!shown)
        {
            if (PreviewMesh != null && PreviewMesh.Visible)
                PreviewMesh.Visible = false;

            return;
        }

        if (PreviewMesh == null || PreviewKey != WallsKey)
        {
            DisposePreview();

            var vertices = new List<Vector3>();
            var indices = new List<int>();

            foreach (var body in Walls.Bodies)
            {
                var start = vertices.Count;
                vertices.AddRange(body.Vertices);
                indices.AddRange(body.Indices.Select(i => i + start));
            }

            var color = CFG.Current.Viewport_FallProtection_Collision_Color;
            var provider = RawMeshProvider.FromTriangles(vertices, indices,
                Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z));

            PreviewMesh = MeshRenderableProxy.FromProvider(scene, provider);
            PreviewMesh.DrawFilter = RenderFilter.Collision;
            PreviewKey = WallsKey;
        }

        var world = Part.GetWorldMatrix();

        if (PreviewMesh.World != world)
            PreviewMesh.World = world;

        if (!PreviewMesh.Visible)
            PreviewMesh.Visible = true;
    }

    private void DisposePreview()
    {
        PreviewMesh?.Dispose();
        PreviewMesh = null;
        PreviewKey = null;
    }

    #endregion

    #region Generate

    /// <summary>
    /// Puts the walls in the part's f file: made first, with no walls, from another file of the binder if the part
    /// has none. The bodies are then replaced (or added to) as one undoable action, and the f binder is written.
    /// </summary>
    private void Generate()
    {
        if (Part == null || Walls == null || Walls.Bodies.Count == 0)
            return;

        var bank = View.HavokCollisionBank;
        var map = (MapContainer)Part.Container;
        var mapId = Part.MapID;
        var fileName = FallFileName;
        var created = false;

        if (!bank.HasMapCollision(mapId, fileName))
        {
            var template = bank.ReadTemplateDocument(mapId, "f", CollisionEditorFallShapes.IsTemplate);

            if (template == null)
            {
                Tool.ReportFailure(GetType(), CollisionEditorSource.Fall, LOC.Get("MAP_CollisionEditor_Fall_No_Template", $"f{mapId.Substring(1)}"));
                return;
            }

            // The new file starts with no walls, so undoing the walls leaves an empty file, as the game has
            CollisionEditorFallShapes.GetSystem(template).m_bodyCinfos.Clear();

            bank.AddMapCollision(mapId, "f", fileName, template);
            Tool.OnCollisionCreated();
            created = true;
        }

        var system = CollisionEditorFallShapes.GetSystem(bank.GetEditableDocument(fileName));

        if (system == null)
        {
            Tool.ReportFailure(GetType(), CollisionEditorSource.Fall, LOC.Get("MAP_CollisionEditor_Fall_Unsupported_File", fileName));
            return;
        }

        var filter = (uint)CFG.Current.MapEditor_CollisionEditor_Fall_Filter;
        var after = CFG.Current.MapEditor_CollisionEditor_Fall_Keep_Existing
            ? new List<Havok.hk2018.hknpPhysicsSystemData.bodyCinfoWithAttachment>(system.m_bodyCinfos)
            : new List<Havok.hk2018.hknpPhysicsSystemData.bodyCinfoWithAttachment>();

        try
        {
            after.AddRange(Walls.Bodies.Select(b => CollisionEditorFallShapes.MakeBody(b, filter, MotionPropertiesId)));
        }
        catch (Exception e)
        {
            Tool.ReportFailure(GetType(), CollisionEditorSource.Fall, LOC.Get("MAP_CollisionEditor_Fall_Build_Failed", fileName), e);
            return;
        }

        View.ViewportActionManager.ExecuteAction(new FallProtectionAction(map, fileName, system.m_bodyCinfos, after,
            name => Tool.OnBodiesChanged(mapId, name)));

        if (!bank.SaveMapCollision(mapId, "f"))
        {
            Tool.ReportFailure(GetType(), CollisionEditorSource.Fall, LOC.Get("MAP_CollisionEditor_Fall_Save_Failed", fileName));
            return;
        }

        // A new file has no loaded mesh yet: it is read from the binder just written
        if (created)
            LoadCollision(map, $"h{ModelNumber}", fileName);

        Tool.ReportSuccess(GetType(), CollisionEditorSource.Fall, true, LOC.Get("MAP_CollisionEditor_Fall_Generated", fileName, Walls.TriangleCount / 2,
            Walls.Bodies.Count, after.Count));
    }

    /// <summary>
    /// Loads the new f file's mesh, by name, as the Create tab loads a new collision's, then gives the parts drawing
    /// it new meshes so they pick it up.
    /// </summary>
    private void LoadCollision(MapContainer map, string modelName, string fileName)
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
