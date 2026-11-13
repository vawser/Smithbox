using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class PropertiesActionTool
{
    public MapEditorView View;
    public ProjectEntry Project;

    public PropertiesActionTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Property Actions section: one-click property writes to the current selection.
    /// </summary>
    public void Display()
    {
        ImGui.BeginTabBar("PropertiesActionTabs");

        DisplayTab();
        LogicTab();
        GameVisibilityTab();

        ImGui.EndTabBar();
    }

    /// <summary>
    /// Reset Transform section, shown in the Transform category beside the other transform tools.
    /// </summary>
    public void DisplayResetTransform()
    {
        GUI.WrappedText("Use this to quickly reset the transform properties of the current selection.");

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("transformActions",
            "resetPosition",
            "Reset Position",
            "Set the position properties of the current selection to <0,0,0>",
            ResetPosition,

            "resetRotation",
            "Reset Rotation",
            "Reset the rotation properties of the current selection to <0,0,0>",
            ResetRotation,

            "resetScale",
            "Reset Scale",
            "Reset the scale properties of the current selection to <1,1,1>",
            ResetScale);
    }

    public void GameVisibilityTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("MAP_ToolWindow_Property_Actions_Game_Visibility_Tab")}##GameVisibilityTab"))
        {
            View.GameVisibilityAction.OnToolWindow();

            ImGui.EndTabItem();
        }
    }

    public void DisplayTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("MAP_ToolWindow_Property_Actions_Display_Tab")}##DisplayTab"))
        {
            GUI.WrappedText("Use this to quickly set display and draw group properties for the current selection.");
            GUI.Spacer();

            GUI.MultiButtonInput("displayActions",
                "clearDisplayGroups",
                "Clear Display Groups",
                "Set all display group entries to 0",
                ClearDisplayGroups,

                "clearDrawGroups",
                "Clear Draw Groups",
                "Set all draw group entries to 0",
                ClearDrawGroups,

                "clearCollisionMaskGroups",
                "Clear Collision Masks",
                "Set all collision mask entries to 0",
                ClearCollisionMasks);

            ImGui.EndTabItem();
        }
    }
    public void LogicTab()
    {
        if (ImGui.BeginTabItem($"{LOC.Get("MAP_ToolWindow_Property_Actions_Logic_Tab")}##LogicTab"))
        {
            GUI.WrappedText("Use this to quickly set Logic type properties for the current selection.");
            GUI.Spacer();

            GUI.MultiButtonInput("logicActions",
                "clearEntityID",
                "Clear Entity ID",
                "Set the Entity ID entry to 0.",
                ClearEntityID,

                "clearEntityGroupIDs",
                "Clear Entity Group IDs",
                "Set all entity group ID entries to 0.",
                ClearEntityGroupIDs);

            ImGui.EndTabItem();
        }
    }


    public void ResetPosition()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            actlist.Add(GetTransformChange(sel, "Position", new Vector3(0, 0, 0)));
        }

        var action = new ViewportCompoundAction(actlist);
        View.ViewportActionManager.ExecuteAction(action);
    }
    public void ResetRotation()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            actlist.Add(GetTransformChange(sel, "Rotation", new Vector3(0, 0, 0)));
        }

        var action = new ViewportCompoundAction(actlist);
        View.ViewportActionManager.ExecuteAction(action);
    }

    public void ResetScale()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            actlist.Add(GetTransformChange(sel, "Scale", new Vector3(1, 1, 1)));
        }

        var action = new ViewportCompoundAction(actlist);
        View.ViewportActionManager.ExecuteAction(action);
    }

    public PropChangeAction GetTransformChange(Entity sel, string propName, Vector3 resetValue)
    {
        // Built with the entity, so the change marks its map as modified and is saved
        PropChangeAction act = new(sel, sel.WrappedObject);

        PropertyInfo prop = sel.WrappedObject.GetType().GetProperty(propName);

        if (prop != null)
            act.AddPropertyChange(prop, resetValue);

        act.SetPostExecutionAction(undo =>
        {
            sel.UpdateRenderModel();
        });

        return act;
    }

    public void ClearDisplayGroups()
    {
        ClearGroupArrays(sel => sel.DisplayGroups);
    }

    public void ClearDrawGroups()
    {
        ClearGroupArrays(sel => sel.DrawGroups);
    }

    public void ClearCollisionMasks()
    {
        ClearGroupArrays(sel => sel.CollisionMasks);
    }

    /// <summary>
    /// Zeroes one of the group arrays of every selected object, as one undo step. The array is copied into from
    /// a zeroed copy, so the action still holds the old values for undo. For a part whose groups come from the
    /// collision it names, the array is the collision's, so that is the object marked as modified.
    /// </summary>
    private void ClearGroupArrays(Func<Entity, uint[]> getArray)
    {
        List<ViewportAction> actlist = new();
        var cleared = new HashSet<uint[]>(ReferenceEqualityComparer.Instance);

        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            var groups = getArray(sel);

            // Parts sharing a collision share its array, which needs clearing once
            if (groups == null || !cleared.Add(groups))
                continue;

            actlist.Add(new PropArrayCopyAction(new uint[groups.Length], groups, sel.RenderGroupOwner ?? sel));
        }

        if (actlist.Count == 0)
            return;

        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actlist));
    }

    public void ClearEntityID()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            PropertyInfo prop = sel.WrappedObject?.GetType().GetProperty("EntityID");

            if (prop == null || !prop.CanWrite)
                continue;

            // Built with the entity, so the change marks its map as modified and is saved
            PropChangeAction act = new(sel, sel.WrappedObject);

            // EntityID is an int in the older MSBs and a uint from Elden Ring on, and SetValue doesn't convert
            act.AddPropertyChange(prop, Convert.ChangeType(0, prop.PropertyType));

            actlist.Add(act);
        }

        if (actlist.Count == 0)
            return;

        var action = new ViewportCompoundAction(actlist);
        View.ViewportActionManager.ExecuteAction(action);
    }

    public void ClearEntityGroupIDs()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            var entityGroupIdsProp = PropFinderUtil.FindProperty("EntityGroupIDs", sel.WrappedObject);

            if (entityGroupIdsProp == null)
                continue;

            if (PropFinderUtil.FindPropertyValue(entityGroupIdsProp, sel.WrappedObject) is not uint[] entityGroupIds)
                continue;

            actlist.Add(new PropArrayCopyAction(new uint[entityGroupIds.Length], entityGroupIds, sel));
        }

        if (actlist.Count == 0)
            return;

        // One undo step for the whole selection
        View.ViewportActionManager.ExecuteAction(new ViewportCompoundAction(actlist));
    }
}
