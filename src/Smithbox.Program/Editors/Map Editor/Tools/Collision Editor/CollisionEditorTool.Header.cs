using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The tool's header, drawn above the tabs whichever is open (Collision Editor UX Analysis, R3): whether collision is
/// drawn, what the viewport's clicks and keys do, and edits that haven't been committed, with Commit and Discard. So
/// the state that changes what the viewport does, and what isn't in the project yet, is always visible.
/// </summary>
public partial class CollisionEditorTool
{
    private void DisplayHeader()
    {
        // Viewport
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_CollisionEditor_Viewport_Header"), LOC.Get("MAP_CollisionEditor_Viewport_Header_TT"));

        var collisionShown = DisplayCollisionShown();
        DisplayModeStatus(collisionShown);
        DisplayPendingStatus();
        DisplayLastResult(null);
    }

    /// <summary>
    /// The last action's result. With a tab, only that tab's own result, drawn under its action button. Without one
    /// (the header): the Edit tab's, as Commit and Discard are in the header, and another tab's, so it is still seen
    /// after switching tabs; the open tab's own is left to the tab. A result that wrote files says when an undo
    /// reaches them.
    /// </summary>
    public void DisplayLastResult(CollisionEditorSource? tab)
    {
        var result = LastResult;

        if (result == null)
            return;

        if (tab == null)
        {
            if (result.Source != CollisionEditorSource.Edit && result.Source == OpenTab)
                return;
        }
        else if (result.Source != tab)
        {
            return;
        }

        if (ImGui.Button($"{Icons.Times}##collisionEditorResultClear{tab}"))
        {
            LastResult = null;
            return;
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Result_Clear_TT"));
        ImGui.SameLine();

        var color = result.Failed ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Benefit_Text_Color;
        GUI.WrappedTextColored(color, LOC.Get("MAP_CollisionEditor_Result", result.Time.ToString("HH:mm:ss"), result.Text));

        if (!result.Failed && result.Wrote)
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Save_Note"));

        // After a commit (shown in the header), the walls that no longer follow the edited edges
        if (tab == null && result.Source == CollisionEditorSource.Edit && StaleFallWalls is { Count: > 0 } stale)
        {
            for (var i = 0; i < stale.Count; i++)
            {
                var (part, model, layer) = stale[i];

                // Deleted, or its map unloaded, since the commit
                if (part.Container is not MapContainer map || !map.Objects.Contains(part))
                    continue;

                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_CollisionEditor_HandOff_Stale_Walls", model));

                GUI.MultiButtonInput($"collisionEditorStaleWalls{i}",
                    $"collisionEditorStaleWallsButton{i}", LOC.Get("MAP_CollisionEditor_HandOff_Regenerate", model),
                    LOC.Get("MAP_CollisionEditor_HandOff_Regenerate_TT"),
                    () => HandOffToFall(part, layer));
            }
        }
    }

    /// <summary>The viewport's collision filter, as the Filters menu sets it. Returns whether collision is drawn.</summary>
    private bool DisplayCollisionShown()
    {
        DisplayGlossaryButton();

        var scene = View.ViewportHandler.ActiveViewport?.RenderScene;

        if (scene == null)
            return false;

        ImGui.SameLine();

        var shown = scene.DrawFilter.HasFlag(RenderFilter.Collision);

        if (ImGui.Checkbox($"{LOC.Get("MAP_CollisionEditor_Show_Collision")}##collisionEditorShowCollision", ref shown))
        {
            scene.SetDrawFilter(RenderFilter.Collision, shown);
            View.BasicFilters.StoreFilterState();
            View.DelayPicking();
        }
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Show_Collision_TT"));

        return shown;
    }

    /// <summary>What the viewport's clicks, keys and gizmo act on, with a button that stops it.</summary>
    private void DisplayModeStatus(bool collisionShown)
    {
        var userDefined = Create.UserDefined;
        string mode = null;

        if (Active)
        {
            mode = LOC.Get("MAP_CollisionEditor_Mode_Edit",
                LOC.Get(CFG.Current.MapEditor_CollisionEditor_Layer.GetDisplayName()),
                InputManager.GetMouseHint(MousebindID.Viewport_Picking_Action),
                LOC.Get(CFG.Current.MapEditor_CollisionEditor_Brush_Mode.GetDisplayName()),
                $"{CFG.Current.MapEditor_CollisionEditor_Brush_Radius:0.###}");
        }
        else if (userDefined.IsShown && userDefined.Placing)
        {
            mode = LOC.Get("MAP_CollisionEditor_Mode_Placing");
        }
        else if (userDefined.GizmoTarget.HasTarget)
        {
            mode = LOC.Get("MAP_CollisionEditor_Mode_Point_Gizmo", userDefined.Selected + 1);
        }

        if (mode == null)
        {
            GUI.WrappedText(LOC.Get("MAP_CollisionEditor_Mode_None"));
            return;
        }

        if (ImGui.Button($"{Icons.Stop}##collisionEditorModeStop"))
            StopModes();
        GUI.Tooltip(LOC.Get("MAP_CollisionEditor_Mode_Stop_TT"));
        ImGui.SameLine();

        GUI.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, mode);

        // The cursor only lands on drawn collision; without it, the keys are taken but do nothing
        if (Active && !collisionShown)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("MAP_CollisionEditor_Mode_Collision_Hidden"));
    }

    /// <summary>
    /// Turns off everything that takes the viewport's clicks, keys or gizmo: the edit cursor, User-Defined point
    /// placing and the point the gizmo holds. Pending edits and placed points are kept.
    /// </summary>
    private void StopModes()
    {
        StopEditing();
        Create.UserDefined.StopModes();
    }

    /// <summary>
    /// Edits that haven't been committed, with Commit and Discard, shown on every tab so they aren't forgotten.
    /// </summary>
    private void DisplayPendingStatus()
    {
        if (!HasEdits)
            return;

        var edited = Files.Values.Where(f => f.IsEdited).ToList();

        GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color,
            LOC.Get("MAP_CollisionEditor_Pending_Status", edited.Count, edited.Sum(CountMovedVertices)));

        GUI.ConditionalMultiButtonInput("collisionEditorActions",
            "collisionEditorCommit", LOC.Get("MAP_CollisionEditor_Commit"), LOC.Get("MAP_CollisionEditor_Commit_TT"),
            CommitChanges, true,

            "collisionEditorDiscard", LOC.Get("MAP_CollisionEditor_Discard"), LOC.Get("MAP_CollisionEditor_Discard_TT"),
            DiscardChanges, true);
    }

    /// <summary>How many of a file's vertices are away from where they were decoded.</summary>
    private static int CountMovedVertices(CollisionEditorFile file)
    {
        return file.Bodies.Sum(b => b.Vertices.Where((v, i) => v != b.OriginalVertices[i]).Count());
    }
}
