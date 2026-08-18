using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
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

    public void Display()
    {
        ImGui.BeginChild("PropertiesActionSection", ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("PropertiesActionTabs");

        TransformTab();
        DisplayTab();
        LogicTab();

        ImGui.EndTabBar();

        ImGui.EndChild();
    }

    public void TransformTab()
    {
        if (ImGui.BeginTabItem("Transform##TransformTab"))
        {
            GUI.WrappedText("Use this to quickly set Transform type properties for the current selection.");
            GUI.Spacer();

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

            ImGui.EndTabItem();
        }
    }

    public void DisplayTab()
    {
        if (ImGui.BeginTabItem("Display##DisplayTab"))
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
        if (ImGui.BeginTabItem("Logic##LogicTab"))
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
        PropChangeAction act = new(sel.WrappedObject);

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
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            uint[] displayGroups = null;

            displayGroups = sel.DisplayGroups;

            if (displayGroups != null)
            {
                for (var i = 0; i < displayGroups.Length; i++)
                {
                    displayGroups[i] = 0;
                }

                PropArrayCopyAction action = new(displayGroups, sel.DisplayGroups);

                View.ViewportActionManager.ExecuteAction(action);
            }
        }
    }

    public void ClearDrawGroups()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            uint[] drawGroups = null;

            drawGroups = sel.DrawGroups;

            if (drawGroups != null)
            {
                for (var i = 0; i < drawGroups.Length; i++)
                {
                    drawGroups[i] = 0;
                }

                PropArrayCopyAction action = new(drawGroups, sel.DrawGroups);

                View.ViewportActionManager.ExecuteAction(action);
            }
        }
    }

    public void ClearCollisionMasks()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            uint[] collisionMasks = null;

            collisionMasks = sel.CollisionMasks;

            if (collisionMasks != null)
            {
                for (var i = 0; i < collisionMasks.Length; i++)
                {
                    collisionMasks[i] = 0;
                }

                PropArrayCopyAction action = new(collisionMasks, sel.CollisionMasks);

                View.ViewportActionManager.ExecuteAction(action);
            }
        }
    }

    public void ClearEntityID()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            PropChangeAction act = new(sel.WrappedObject);

            PropertyInfo prop = sel.WrappedObject.GetType().GetProperty("EntityID");

            if (prop != null)
                act.AddPropertyChange(prop, (uint)0);

            actlist.Add(act);
        }

        var action = new ViewportCompoundAction(actlist);
        View.ViewportActionManager.ExecuteAction(action);
    }

    public void ClearEntityGroupIDs()
    {
        List<ViewportAction> actlist = new();
        foreach (Entity sel in View.ViewportSelection.GetFilteredSelection<Entity>())
        {
            var entityGroupIdsProp = PropFinderUtil.FindProperty("EntityGroupIDs", sel.WrappedObject);

            if (entityGroupIdsProp != null)
            {
                var entityGroupIds = (uint[])PropFinderUtil.FindPropertyValue(entityGroupIdsProp, sel.WrappedObject);


                uint[] newEntityGroupIds = (uint[])entityGroupIds.Clone();
                for(int i = 0; i < newEntityGroupIds.Length; i++)
                {
                    newEntityGroupIds[i] = 0;
                }

                PropArrayCopyAction action = new(newEntityGroupIds, entityGroupIds);

                View.ViewportActionManager.ExecuteAction(action);
            }
        }

    }
}
