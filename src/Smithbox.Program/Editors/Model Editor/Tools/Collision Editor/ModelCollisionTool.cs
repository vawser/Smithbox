using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Veldrid.Utilities;

namespace StudioCore.Editors.ModelEditor;

/// <summary>The tab of the Model Editor's Collision tool.</summary>
public enum ModelCollisionTab
{
    Edit,
    Material,
    Create
}

/// <summary>
/// The Model Editor's Collision tool: edits the collision of the loaded model (an asset's or object's), as the Map
/// Editor's Collision Editor edits map collision. The Edit tab reshapes it with the vertex brush, the Material tab shows
/// and paints the material of each face, and the Create tab builds it from the model's own FLVER meshes. Only the high
/// and low layers: models have no fall protection.
/// <para>
/// The chosen layer is decoded into the Collision Editor's bodies (<see cref="CollisionEditorBody" />) and drawn from
/// them while the tool is open, in place of the collision the model is shown with (which may be the other layer).
/// Moved vertices and repainted faces are pending until they are committed; a commit, and a Create, rebuild the
/// bodies' shapes as one undoable <see cref="ModelCollisionAction" /> and mark the model as changed, and the model's
/// save writes the collision files (<see cref="ModelCollisionFiles" />).
/// </para>
/// </summary>
public partial class ModelCollisionTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;

    // The viewport frames, counted by HandleViewportInput; the tool and its tabs note the frame they were drawn in
    private int Frame;
    private int LastToolFrame = -10;

    private ModelCollisionTab OpenTab = ModelCollisionTab.Edit;

    /// <summary>A decoded layer of the loaded model: its file and bodies, which edits change until they are committed.</summary>
    private sealed class DecodedLayer
    {
        public ModelCollisionFile File;
        public List<CollisionEditorBody> Bodies;
        public bool IsEdited => Bodies.Any(b => b.IsEdited);
    }

    // The model the decoded layers are of, and the layers decoded so far
    private ModelWrapper Wrapper;
    private readonly Dictionary<HavokCollisionType, DecodedLayer> Layers = new();

    // Layers whose file has nothing the tool can edit, so they aren't decoded again every frame
    private readonly HashSet<HavokCollisionType> Unsupported = new();

    // The collision mesh the model is shown with, hidden while the tool draws the layer itself
    private MeshRenderableProxy HiddenMesh;

    // Collision meshes replaced by a commit, freed a few frames later so nothing draws from them when they go
    private const int ReleaseDelayFrames = 10;
    private readonly List<(CollisionSubmesh[] Meshes, int Frame)> PendingRelease = new();

    private record Hit(CollisionEditorBody Body, int Triangle, int Vertex, Vector3 Point, float Distance);

    // The last commit, discard or create, shown in the header so it is seen without the log
    private (string Text, bool Failed, DateTime Time)? LastResult;

    public ModelCollisionTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private bool ToolOpen => Frame - LastToolFrame <= 1;

    private static HavokCollisionType Layer
    {
        get => CFG.Current.ModelEditor.ModelEditor_CollisionTool_Layer is HavokCollisionType.High ? HavokCollisionType.High : HavokCollisionType.Low;
        set => CFG.Current.ModelEditor.ModelEditor_CollisionTool_Layer = value;
    }

    /// <summary>The loaded model of this view, if any.</summary>
    private ModelWrapper GetWrapper()
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        return wrapper?.Container != null && wrapper.Container.View == View ? wrapper : null;
    }

    private void ReportSuccess(string text)
    {
        Smithbox.Log(this, text);
        LastResult = (text, false, DateTime.Now);
    }

    private void ReportFailure(string text, Exception exception = null)
    {
        Smithbox.LogError(this, text, exception);
        LastResult = (text, true, DateTime.Now);
    }

    #region Tool Window

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MODEL_CollisionTool_Header")}##modelCollisionToolHeader"))
            return;

        ImGui.BeginChild("modelCollisionToolSection", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        LastToolFrame = Frame;

        if (Smithbox.Instance.CurrentBackend is not RenderingBackend.Vulkan)
        {
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Needs_Vulkan"));
            return;
        }

        GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Description"));

        var wrapper = GetWrapper();
        SyncWrapper(wrapper);

        if (wrapper == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_No_Model"));
            return;
        }

        DisplayHeader(wrapper);

        GUI.Spacer();

        if (ImGui.BeginTabBar("modelCollisionToolTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_CollisionEditor_Tab_Edit")}##modelCollisionTabEdit"))
            {
                OpenTab = ModelCollisionTab.Edit;
                DisplayEditTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_CollisionEditor_Tab_Material")}##modelCollisionTabMaterial"))
            {
                OpenTab = ModelCollisionTab.Material;
                DisplayMaterialTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_CollisionEditor_Tab_Create")}##modelCollisionTabCreate"))
            {
                OpenTab = ModelCollisionTab.Create;
                DisplayCreateTab(wrapper);
                ImGui.EndTabItem();
            }

            var (name, description, help) = OpenTab switch
            {
                ModelCollisionTab.Material => ("MAP_CollisionEditor_Tab_Material", "MAP_CollisionEditor_Material_Description", "MODEL_CollisionTool_Help_Material"),
                ModelCollisionTab.Create => ("MAP_CollisionEditor_Tab_Create", "MODEL_CollisionTool_Create_Description", "MODEL_CollisionTool_Help_Create"),
                _ => ("MAP_CollisionEditor_Tab_Edit", "MODEL_CollisionTool_Edit_Description", "MODEL_CollisionTool_Help_Edit"),
            };

            GUI.TabHelpButton("modelCollisionTabHelp", LOC.Get(name), LOC.Get(description), LOC.Get(help));

            ImGui.EndTabBar();
        }
    }

    /// <summary>
    /// Above the tabs: whether collision is drawn, the layer edited, what the viewport's clicks do, and the edits that
    /// haven't been committed, with Commit and Discard.
    /// </summary>
    private void DisplayHeader(ModelWrapper wrapper)
    {
        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Viewport_Header"), LOC.Get("MAP_CollisionEditor_Viewport_Header_TT"));

        var shown = View.RenderScene.DrawFilter.HasFlag(RenderFilter.Collision);

        if (ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Show_Collision")}##modelCollisionShowCollision", ref shown))
            View.RenderScene.SetDrawFilter(RenderFilter.Collision, shown);
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Show_Collision_TT"));

        DisplayModeStatus(shown);

        // Layer
        var files = wrapper.CollisionFiles;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Layer_Header"), LOC.Get("MODEL_CollisionTool_Layer_Header_TT"));

        if (files.UsesLayerBinders)
        {
            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##modelCollisionLayer", LOC.Get(Layer.GetDisplayName())))
            {
                foreach (var type in files.Layers)
                {
                    if (ImGui.Selectable($"{LOC.Get(type.GetDisplayName())}##modelCollisionLayer{type}", type == Layer))
                    {
                        Layer = type;
                        StopModes();
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("MODEL_CollisionTool_Layer_Input_TT"));
        }

        var layer = GetLayer();
        var file = files.Get(files.UsesLayerBinders ? Layer : HavokCollisionType.Low);

        if (file == null && !files.UsesLayerBinders && wrapper.HKX_ReadFailed)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_CollisionTool_Layer_Read_Failed", wrapper.HKX_Name));
        else if (file == null)
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Layer_None", files.GetEntryName(files.UsesLayerBinders ? Layer : HavokCollisionType.Low)));
        else if (layer == null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_CollisionTool_Layer_Unsupported", file.EntryName));
        else
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Layer_File", file.EntryName, layer.Bodies.Sum(b => b.TriangleCount),
                file.BinderPath ?? wrapper.Parent.Path));

        // Pending edits, with Commit and Discard
        var edited = Layers.Values.Where(l => l.IsEdited).ToList();

        if (edited.Count > 0)
        {
            GUI.Spacer();
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_CollisionEditor_Pending_Status",
                edited.Count, edited.Sum(CountMovedVertices), edited.Sum(l => l.Bodies.Sum(b => b.RepaintedCount))));

            GUI.ConditionalMultiButtonInput("modelCollisionActions",
                "modelCollisionCommit", LOC.Get("MAP_CollisionEditor_Commit"), LOC.Get("MODEL_CollisionTool_Commit_TT"),
                CommitChanges, true,

                "modelCollisionDiscard", LOC.Get("MAP_CollisionEditor_Discard"), LOC.Get("MAP_CollisionEditor_Discard_TT"),
                DiscardChanges, true);
        }

        if (files.HasModified)
            GUI.WrappedText(LOC.Get("MODEL_CollisionTool_Unsaved"));

        if (LastResult is { } result)
        {
            if (ImGui.Button($"{Icons.Times}##modelCollisionResultClear"))
            {
                LastResult = null;
                return;
            }
            GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Result_Clear_TT"));
            ImGui.SameLine();

            var color = result.Failed ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Benefit_Text_Color;
            GUI.WrappedTextColored(color, LOC.Get("MAP_CollisionEditor_Result", result.Time.ToString("HH:mm:ss"), result.Text));
        }
    }

    /// <summary>What the viewport's clicks and keys do, with a button that stops it.</summary>
    private void DisplayModeStatus(bool collisionShown)
    {
        string mode = null;

        if (EditActive)
        {
            mode = LOC.Get("MAP_CollisionEditor_Mode_Edit", LOC.Get(Layer.GetDisplayName()),
                InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action),
                LOC.Get(CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Mode.GetDisplayName()),
                $"{CFG.Current.MapEditor.MapEditor_CollisionEditor_Brush_Radius:0.###}");
        }
        else if (MaterialActive)
        {
            mode = MaterialMode is CollisionMaterialMode.Select
                ? LOC.Get("MAP_CollisionEditor_Mode_Material_Select", LOC.Get(Layer.GetDisplayName()),
                    InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action))
                : LOC.Get("MAP_CollisionEditor_Mode_Material", LOC.Get(Layer.GetDisplayName()),
                    InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action),
                    LOC.Get(MaterialMode.GetDisplayName()), GetMaterialNames().Describe(GetPaintMaterial()));
        }

        if (mode == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Mode_None"));
            return;
        }

        if (ImGui.Button($"{Icons.Stop}##modelCollisionModeStop"))
            StopModes();
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Mode_Stop_TT"));
        ImGui.SameLine();

        GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, mode);

        if (!collisionShown)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_CollisionEditor_Mode_Collision_Hidden"));
    }

    /// <summary>Turns off the Edit brush and material painting. Pending edits are kept.</summary>
    private void StopModes()
    {
        if (EditActive)
            ToggleEditing();

        if (MaterialActive)
            ToggleMaterialPainting();
    }

    private static int CountMovedVertices(DecodedLayer layer)
    {
        return layer.Bodies.Sum(b => b.Vertices.Where((v, i) => v != b.OriginalVertices[i]).Count());
    }

    /// <summary>The files with uncommitted edits.</summary>
    private void DisplayPendingList()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Pending_Header"), LOC.Get("MAP_CollisionEditor_Pending_Header_TT"));

        var edited = Layers.Values.Where(l => l.IsEdited).ToList();

        if (edited.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Pending_None"));
            return;
        }

        foreach (var layer in edited)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Pending_File", layer.File.EntryName, CountMovedVertices(layer),
                layer.Bodies.Sum(b => b.RepaintedCount)));
        }
    }

    #endregion

    #region Layers

    /// <summary>
    /// Follows the loaded model: when another is loaded, what was decoded for the last one goes (with its uncommitted
    /// edits, which are logged) and its collision mesh is shown again.
    /// </summary>
    private void SyncWrapper(ModelWrapper wrapper)
    {
        if (wrapper == Wrapper)
            return;

        if (Layers.Values.Any(l => l.IsEdited))
            Smithbox.Log(this, LOC.Get("MODEL_CollisionTool_Edits_Dropped", Wrapper?.Name));

        StopModes();
        ReleaseLayers();

        Wrapper = wrapper;
        ResetCreate();
    }

    /// <summary>Drops the decoded layers and their previews, and shows the model's collision mesh again.</summary>
    private void ReleaseLayers()
    {
        foreach (var layer in Layers.Values)
        {
            foreach (var body in layer.Bodies)
                body.DisposePreview();
        }

        Layers.Clear();
        Unsupported.Clear();

        MaterialHover = null;
        SelectedFace = null;
        Hover = null;
        EndStroke();

        MaterialOverlay.Clear();
        DisposeHoverFace();
        DisposeSelectedFaceMesh();
        DisposeCursor();

        ShowDrawnMesh();
    }

    /// <summary>The layer edited, decoded if it hasn't been. Null if the model has no such file or nothing to edit in it.</summary>
    private DecodedLayer GetLayer()
    {
        var wrapper = Wrapper;

        if (wrapper == null)
            return null;

        return GetLayer(wrapper.CollisionFiles.UsesLayerBinders ? Layer : HavokCollisionType.Low);
    }

    /// <summary>
    /// The layers the model has (high and low for assets, the single file for other models). For edits that cover
    /// every layer, such as the material remap.
    /// </summary>
    private IEnumerable<HavokCollisionType> LayerTypes()
    {
        var wrapper = Wrapper;

        if (wrapper == null)
            return Array.Empty<HavokCollisionType>();

        return wrapper.CollisionFiles.UsesLayerBinders
            ? new[] { HavokCollisionType.High, HavokCollisionType.Low }
            : new[] { HavokCollisionType.Low };
    }

    /// <summary>A layer of the model, decoded if it hasn't been. Null if the model has no such file or nothing to edit in it.</summary>
    private DecodedLayer GetLayer(HavokCollisionType type)
    {
        var wrapper = Wrapper;

        if (wrapper == null)
            return null;

        var files = wrapper.CollisionFiles;

        if (Layers.TryGetValue(type, out var layer))
            return layer;

        if (Unsupported.Contains(type))
            return null;

        var file = files.Get(type);

        if (file == null)
            return null;

        var bodies = new List<CollisionEditorBody>();

        try
        {
            foreach (var body in Havok.Shared.CollisionGeometry.Bodies(file.Document.Objects))
            {
                var decoded = CollisionEditorBody.Decode(body);

                if (decoded != null)
                    bodies.Add(decoded);
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MAP_CollisionEditor_Decode_Failed", file.EntryName), e);
            bodies.Clear();
        }

        if (bodies.Count == 0)
        {
            Unsupported.Add(type);
            return null;
        }

        layer = new DecodedLayer { File = file, Bodies = bodies };
        Layers[type] = layer;

        return layer;
    }

    /// <summary>Where the collision is drawn: the model's collision node, which follows the model's display rotation.</summary>
    private Matrix4x4 GetWorld()
    {
        var container = Wrapper?.Container;

        if (container == null)
            return Matrix4x4.Identity;

        return container.Collision?.GetWorldMatrix() ?? container.DisplayMatrix;
    }

    private static Matrix4x4 GetBodyWorld(CollisionEditorBody body, Matrix4x4 world) => body.BodyTransform * world;

    private static Color GetPreviewColor(HavokCollisionType type)
    {
        var color = type is HavokCollisionType.High ? CFG.Current.Viewport.Viewport_High_Collision_Color : CFG.Current.Viewport.Viewport_Collision_Color;

        return Color.FromArgb(255, (int)color.X, (int)color.Y, (int)color.Z);
    }

    #endregion

    #region Viewport

    /// <summary>
    /// Handles the Edit brush and material painting, and draws the layer while the tool is open. Called each frame by
    /// the active viewport. Returns true while the viewport's clicks are taken, so they don't select.
    /// </summary>
    public bool HandleViewportInput(VulkanViewport viewport, Ray ray, bool canInteract)
    {
        Frame++;
        ReleaseReplacedMeshes();

        var scene = View.RenderScene;

        // The tool is drawn after the viewport, so it is open if it was drawn last frame
        if (!ToolOpen || scene == null)
        {
            if (Layers.Count > 0 || HiddenMesh != null || MaterialOverlay.IsShown)
            {
                StopModes();
                ReleaseLayers();
            }

            return false;
        }

        // The model may have been switched since the tool was drawn
        SyncWrapper(GetWrapper());

        if (Wrapper == null)
            return false;

        // Leaving a tab stops what it does with the viewport; pending edits are kept
        if (EditActive && OpenTab != ModelCollisionTab.Edit)
            ToggleEditing();

        if (MaterialActive && OpenTab != ModelCollisionTab.Material)
            ToggleMaterialPainting();

        var layer = GetLayer();

        if (EditActive)
            HandleEditing(scene, layer, ray, canInteract);

        if (MaterialActive)
            HandleMaterialPainting(scene, layer, ray, canInteract);

        UpdatePreview(scene, layer);
        UpdateMaterialOverlay(scene, layer);

        return EditActive || MaterialActive;
    }

    /// <summary>
    /// Draws the layer from its bodies, and once that is drawn hides the collision mesh the model is shown with, which
    /// may be the other layer, or this one before its edits.
    /// </summary>
    private void UpdatePreview(RenderScene scene, DecodedLayer layer)
    {
        // Another layer may have been drawn before
        foreach (var other in Layers.Values.Where(l => l != layer))
        {
            foreach (var body in other.Bodies)
                body.DisposePreview();
        }

        if (layer == null)
        {
            ShowDrawnMesh();
            return;
        }

        var world = GetWorld();
        var color = GetPreviewColor(layer.File.Type);
        var visible = Wrapper.Container.Collision?.EditorVisible ?? true;
        var ready = true;

        foreach (var body in layer.Bodies)
            ready &= body.UpdatePreview(scene, world, color, visible);

        if (ready)
            HideDrawnMesh();
    }

    private void HideDrawnMesh()
    {
        if (Wrapper?.Container?.Collision?.RenderSceneMesh is not MeshRenderableProxy mesh)
            return;

        HiddenMesh = mesh;

        if (mesh.Visible)
            mesh.Visible = false;
    }

    private void ShowDrawnMesh()
    {
        if (HiddenMesh == null)
            return;

        // Shown again as its node is, unless the model it is of has gone
        var collision = Wrapper?.Container?.Collision;

        if (collision?.RenderSceneMesh == HiddenMesh)
            HiddenMesh.Visible = collision.EditorVisible;

        HiddenMesh = null;
    }

    /// <summary>The nearest triangle of the layer under the ray, and the corner of it nearest the hit.</summary>
    private Hit RayCast(DecodedLayer layer, Ray ray)
    {
        if (layer == null)
            return null;

        Hit nearest = null;
        var world = GetWorld();

        foreach (var body in layer.Bodies)
        {
            var bodyWorld = GetBodyWorld(body, world);

            if (!Matrix4x4.Invert(bodyWorld, out var inverse))
                continue;

            var origin = Vector3.Transform(ray.Origin, inverse);
            var direction = Vector3.TransformNormal(ray.Direction, inverse);
            var scale = direction.Length();

            if (scale < 1e-8f)
                continue;

            if (!body.RayCast(new Ray(origin, direction / scale), out var triangle, out var distance))
                continue;

            var localPoint = origin + direction / scale * distance;
            distance /= scale;

            if (nearest == null || distance < nearest.Distance)
            {
                nearest = new Hit(body, triangle, body.NearestCorner(triangle, localPoint),
                    Vector3.Transform(localPoint, bodyWorld), distance);
            }
        }

        return nearest;
    }

    #endregion

    #region Commit

    /// <summary>
    /// Rebuilds the shapes of the edited bodies from their moved vertices and repainted materials, and gives them to
    /// the bodies as one undoable action. The model's save writes them.
    /// </summary>
    private void CommitChanges() => CommitChanges(LOC.Get("MODEL_CollisionTool_History_Commit"), null);

    /// <summary>
    /// The commit, with the history step's name and the success line (null: the commit's own). Returns whether it
    /// committed anything.
    /// </summary>
    private bool CommitChanges(string historyLabel, string successText)
    {
        EndStroke();
        MaterialStroking = false;

        var swaps = new List<ModelCollisionAction.ShapeSwap>();
        var edited = Layers.Values.Where(l => l.IsEdited).ToList();

        foreach (var layer in edited)
        {
            foreach (var body in layer.Bodies.Where(b => b.IsEdited))
            {
                var (vertices, indices, materials, keys) = body.GetBuildInput();

                if (indices.Count == 0)
                {
                    ReportFailure(LOC.Get("MAP_CollisionEditor_Commit_No_Triangles", layer.File.EntryName));
                    return false;
                }

                CollisionEditorShapes.BuiltShape built;

                try
                {
                    // As the Map Editor's commit: the older games' meshes keep their triangles unless one was repainted
                    built = Havok.Builders.HkpMeshShapeRebuild.IsSupported(body.Body.Shape) && !body.IsMaterialEdited
                        ? CollisionEditorShapes.BuildHkpShape(body.Body.Shape, vertices, indices, keys)
                        : CollisionEditorShapes.BuildShape(body.Body.Shape, vertices, indices, materials);
                }
                catch (Exception e)
                {
                    ReportFailure(LOC.Get("MAP_CollisionEditor_Commit_Build_Failed", layer.File.EntryName), e);
                    return false;
                }

                if (built == null)
                {
                    ReportFailure(LOC.Get("MAP_CollisionEditor_Commit_Build_Failed", layer.File.EntryName));
                    return false;
                }

                swaps.Add(new ModelCollisionAction.ShapeSwap(layer.File, body.Body, body.Body.Shape, built.Shape));
            }
        }

        if (swaps.Count == 0)
            return false;

        View.ViewportActionManager.ExecuteAction(new ModelCollisionAction(Wrapper, swaps, null, OnCollisionChanged, historyLabel));

        ReportSuccess(successText ?? LOC.Get("MODEL_CollisionTool_Committed", string.Join(", ", edited.Select(l => l.File.EntryName))));
        return true;
    }

    private void DiscardChanges()
    {
        EndStroke();
        MaterialStroking = false;

        foreach (var layer in Layers.Values)
        {
            foreach (var body in layer.Bodies)
            {
                body.Reset();
                body.DisposePreview();
            }
        }

        ReportSuccess(LOC.Get("MAP_CollisionEditor_Discarded"));
    }

    /// <summary>
    /// After a commit or create, or their undo or redo: the decoded layers are out of date, so they are decoded again
    /// from the documents, and the collision mesh the model is shown with is rebuilt if it is of a changed file.
    /// </summary>
    private void OnCollisionChanged()
    {
        var wrapper = Wrapper;

        foreach (var layer in Layers.Values)
        {
            foreach (var body in layer.Bodies)
                body.DisposePreview();
        }

        Layers.Clear();
        Unsupported.Clear();
        MaterialHover = null;
        SelectedFace = null;
        Hover = null;
        EndStroke();

        if (wrapper != null)
            RefreshDrawnMesh(wrapper);

        // Shown until the layer's preview is drawn again, so the collision doesn't disappear meanwhile
        ShowDrawnMesh();
    }

    /// <summary>Rebuilds the drawn collision mesh from the document it was read from, which a commit has changed.</summary>
    private void RefreshDrawnMesh(ModelWrapper wrapper)
    {
        if (wrapper.HKX_Document == null || wrapper.Container?.Collision?.RenderSceneMesh is not MeshRenderableProxy proxy)
            return;

        var resource = (proxy.ResourceHandle as ResourceHandle<HavokCollisionResource>)?.Get();

        if (resource == null)
            return;

        var old = HavokCollisionMeshBuilder.RefreshCollisionMesh(resource, wrapper.HKX_Document);

        if (old == null)
        {
            Smithbox.Log(this, LOC.Get("MAP_CollisionEditor_Refresh_Failed", wrapper.HKX_Name));
            return;
        }

        PendingRelease.Add((old, Frame));

        if (proxy._meshProvider is CollisionMeshProvider provider)
            provider._bounds = resource.Bounds;

        foreach (var submesh in proxy.Submeshes)
            submesh.ScheduleRenderableConstruction();
    }

    private void ReleaseReplacedMeshes()
    {
        for (var i = PendingRelease.Count - 1; i >= 0; i--)
        {
            if (Frame - PendingRelease[i].Frame < ReleaseDelayFrames)
                continue;

            foreach (var mesh in PendingRelease[i].Meshes)
                mesh.GeomBuffer?.Dispose();

            PendingRelease.RemoveAt(i);
        }
    }

    #endregion
}
