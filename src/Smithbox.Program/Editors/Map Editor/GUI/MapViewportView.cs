using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.Utilities;
using Vortice.Vulkan;

namespace StudioCore.Editors.MapEditor;

public class MapViewportView
{
    public MapEditorView View;
    public ProjectEntry Project;

    public Rectangle Rect;

    public bool AltHeld;
    public bool CtrlHeld;
    public bool ShiftHeld;
    public bool ViewportUsingKeyboard;

    public PlacementEntity PlacementOrb;

    public readonly MapLightCollector LightCollector;

    public MapViewportView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        LightCollector = new MapLightCollector(view);

        view.Window = Smithbox.Instance._context.Window;
        view.Device = Smithbox.Instance._context.Device;

        Rect = view.Window.Bounds;

    }

    public void Display(uint dockspaceId)
    {
        foreach (var viewport in View.ViewportHandler.Viewports)
        {
            if (viewport == null)
            {
                continue;
            }

            ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
            ImGui.SetNextWindowClass(ref GUI.DockGroup_MapEditorView);

            if (viewport.Viewport is VulkanViewport vulkanViewport)
            {
                vulkanViewport.Display();

                if (View.Universe != null && PlacementOrb == null)
                {
                    PlacementOrb = new PlacementEntity(View.Universe);
                }

                if (PlacementOrb != null)
                {
                    PlacementOrb.UpdateRenderModel();
                }
            }

            if (viewport.Viewport is NullViewport nullViewport)
            {
                nullViewport.Display();
            }
        }

        ViewportContextMenu();
    }

    public void Update(float deltatime)
    {
        LightCollector.Update();
        View.EnemyAIVisualizationTool.Update();

        // The viewport keeps its last focus and bounds whilst hidden, so it must not respond to input
        if (View.ShowMapData)
        {
            ViewportUsingKeyboard = false;
            return;
        }

        foreach (var viewport in View.ViewportHandler.Viewports)
        {
            if (viewport == null)
                continue;

            if (viewport.Viewport != null)
            {
                if (viewport == View.ViewportHandler.ActiveViewport)
                {
                    ViewportUsingKeyboard = viewport.Viewport.Update(View.Window, deltatime);
                }
                else
                {
                    viewport.Viewport.Update(View.Window, deltatime);
                }
            }
        }
    }

    public void EditorResized(Sdl2Window window, GraphicsDevice device)
    {
        View.Window = window;
        Rect = window.Bounds;
    }

    public void Draw(GraphicsDevice device, CommandList cl)
    {
        foreach (var viewport in View.ViewportHandler.Viewports)
        {
            viewport.Viewport.Draw(device, cl);
        }
    }

    public bool InputCaptured()
    {
        if (View.ShowMapData)
            return false;

        var activeViewport = View.ViewportHandler.ActiveViewport;

        if (activeViewport.Viewport != null)
        {
            return activeViewport.Viewport.IsViewportSelected;
        }

        return false;
    }

    public Vector3 GetPlacementPosition()
    {
        var activeViewport = View.ViewportHandler.ActiveViewport;

        if(activeViewport.Viewport == null)
            return Vector3.Zero;

        if (View.Device != null)
        {
            // Get the camera's view matrix and position
            var viewMatrix = activeViewport.Viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH;
            var cameraPosition = activeViewport.Viewport.ViewportCamera.CameraTransform.Position;

            // Invert the view matrix to get the camera's world matrix
            Matrix4x4.Invert(viewMatrix, out var cameraWorldMatrix);

            // Extract the forward direction from the world matrix (Z axis)
            Vector3 forward = new Vector3(cameraWorldMatrix.M31, cameraWorldMatrix.M32, cameraWorldMatrix.M33);
            Vector3 forwardNormalized = Vector3.Normalize(forward);

            // Calculate the position in front of the camera using configured distance
            Vector3 placementPosition = cameraPosition + forwardNormalized * CFG.Current.PlacementOrb_Distance;

            return placementPosition;
        }

        return Vector3.Zero;
    }

    /// <summary>
    /// Get the placement transform. For the placement of the orb in the screenspace.
    /// </summary>
    /// <returns></returns>
    public Matrix4x4 GetPlacementTransform()
    {
        var activeViewport = View.ViewportHandler.ActiveViewport;

        if (activeViewport.Viewport == null)
            return Matrix4x4.Identity;

        if (View.Device != null)
        {
            // Get the camera's view matrix and position
            var viewMatrix = activeViewport.Viewport.ViewportCamera.CameraTransform.CameraViewMatrixLH;
            var cameraPosition = activeViewport.Viewport.ViewportCamera.CameraTransform.Position;

            Matrix4x4.Invert(viewMatrix, out var cameraWorldMatrix);

            // Extract the forward direction from the view matrix
            Vector3 forward = new Vector3(cameraWorldMatrix.M31, cameraWorldMatrix.M32, cameraWorldMatrix.M33);
            Vector3 forwardNormalized = Vector3.Normalize(forward);

            // Calculate the placement position 5 units in front
            Vector3 placementPosition = cameraPosition + forwardNormalized * CFG.Current.PlacementOrb_Distance;

            // Reconstruct the rotation from the view matrix
            Vector3 up = new Vector3(cameraWorldMatrix.M21, cameraWorldMatrix.M22, cameraWorldMatrix.M23);
            Vector3 right = new Vector3(cameraWorldMatrix.M11, cameraWorldMatrix.M12, cameraWorldMatrix.M13);

            // Create world matrix using right, up, forward, and position
            Matrix4x4 placementTransform = new Matrix4x4(
                right.X, right.Y, right.Z, 0.0f,
                up.X, up.Y, up.Z, 0.0f,
                forward.X, forward.Y, forward.Z, 0.0f,
                placementPosition.X, placementPosition.Y, placementPosition.Z, 1.0f
            );

            return placementTransform;
        }

        return Matrix4x4.Identity;
    }

    public void ViewportContextMenu()
    {
        if (!FocusManager.IsFocus(EditorFocusContext.MapEditor_Viewport))
            return;

        if(InputManager.IsMousePressed(MousebindID.MapEditor_Display_Viewport_Context_Menu))
        {
            var mousePos = InputManager.MousePosition;
            ImGui.SetNextWindowPos(mousePos, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

            ImGui.OpenPopup("Viewport Context Menu");
        }

        var curSelection = View.ViewportSelection.GetSelection();

        if (curSelection.Count == 0)
        {
            if (ImGui.BeginPopup("Viewport Context Menu"))
            {
                EmptySelectionContextMenu();

                ImGui.EndPopup();
            }
        }
        else
        {
            if (ImGui.BeginPopup("Viewport Context Menu"))
            {
                // The items act on the whole selection; this entity only decides which are shown. A part or
                // region is preferred, as most items apply to those, so a selection that starts with an event
                // still offers them. The selection can also hold non-entities (a reference to an unloaded map),
                // in which case the entity-specific items are skipped.
                Entity targetedEnt = curSelection.OfType<Entity>().FirstOrDefault(e => e.WrappedObject is IMsbPart or IMsbRegion)
                    ?? curSelection.OfType<Entity>().FirstOrDefault();

                View.DuplicateAction.OnContext();
                View.DeleteAction.OnContext();
                View.DuplicateToMapAction.OnContext();
                View.TranslateAction.OnContext();
                View.RotateAction.OnContext();

                if (targetedEnt != null)
                {
                    View.ScrambleAction.OnContext(targetedEnt);
                    View.ReplicateAction.OnContext(targetedEnt);

                    ImGui.Separator();

                    View.FrameAction.OnContext(targetedEnt);
                    View.PullToCameraAction.OnContext(targetedEnt);
                    View.SnapToMeshAction.OnContext(targetedEnt);
                }

                ImGui.Separator();

                View.EditorVisibilityAction.OnContext();
                View.GameVisibilityAction.OnContext();

                if (targetedEnt != null)
                {
                    ImGui.Separator();

                    View.SelectAllAction.OnContext(targetedEnt);
                    View.SelectCollisionRefAction.OnContext(targetedEnt);
                }

                ImGui.Separator();

                View.AdjustToGridAction.OnContext();

                if (targetedEnt != null)
                {
                    ImGui.Separator();

                    View.EntityInfoAction.OnContext(targetedEnt);
                }

                ImGui.EndPopup();
            }
        }
    }

    /// <summary>
    /// The context menu when nothing is selected: actions on the active map rather than on a selection
    /// </summary>
    private void EmptySelectionContextMenu()
    {
        var map = View.Selection.SelectedMapContainer;

        View.CreateAction.OnMenu();

        if (ImGui.Selectable(LOC.Get("MAP_Viewport_Context_Paste"), false,
            View.DuplicateToMapAction.HasCopiedObjects ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            View.DuplicateToMapAction.OnPaste(View);
        }
        GUI.Tooltip(LOC.Get("MAP_Viewport_Context_Paste_TT", InputManager.GetHint(KeybindID.Paste)));

        ImGui.Separator();

        if (ImGui.Selectable(LOC.Get("MAP_Viewport_Context_Frame_Map"), false,
            map != null ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            FrameMap(map);
        }
        GUI.Tooltip(LOC.Get("MAP_Viewport_Context_Frame_Map_TT"));

        if (ImGui.Selectable(LOC.Get("MAP_Viewport_Context_Show_All"), false,
            map != null ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            foreach (var entry in map.Objects)
            {
                entry.EditorVisible = true;
            }
        }
        GUI.Tooltip(LOC.Get("MAP_Viewport_Context_Show_All_TT"));
    }

    /// <summary>
    /// Frames the bounds of every drawn object of the map
    /// </summary>
    private void FrameMap(MapContainer map)
    {
        var viewport = View.ViewportHandler.ActiveViewport?.Viewport;

        if (viewport == null)
            return;

        BoundingBox? bounds = null;

        foreach (var ent in map.Objects)
        {
            if (ent.RenderSceneMesh == null || !ent.EditorVisible)
                continue;

            var box = ent.RenderSceneMesh.GetBounds();
            bounds = bounds is BoundingBox b ? BoundingBox.Combine(b, box) : box;
        }

        if (bounds is BoundingBox all)
        {
            viewport.FrameBox(all, new Vector3());
        }
        else if (map.RootObject != null)
        {
            viewport.FramePosition(map.RootObject.GetLocalTransform().Position, 10f);
        }
    }
}
