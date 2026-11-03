using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class GameVisibilityAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public GameVisibilityAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Make_Dummy_Object))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }

            if (InputManager.IsPressed(KeybindID.MapEditor_Make_Normal_Object))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }

            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                if (InputManager.IsPressed(KeybindID.MapEditor_Disable_Game_Presence))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                }

                if (InputManager.IsPressed(KeybindID.MapEditor_Enable_Game_Presence))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
                }
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.BeginMenu("Game Visibility"))
        {
            if (ImGui.MenuItem("Set Map Object to Dummy", InputManager.GetHint(KeybindID.MapEditor_Make_Dummy_Object)))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Set Map Object to Normal", InputManager.GetHint(KeybindID.MapEditor_Make_Normal_Object)))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }

            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                if (ImGui.MenuItem("Disable Map Object Game Presence", InputManager.GetHint(KeybindID.MapEditor_Disable_Game_Presence)))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Enable Map Object Game Presence", InputManager.GetHint(KeybindID.MapEditor_Enable_Game_Presence)))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
                }
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Game Visibility"))
        {
            if (ImGui.MenuItem("Set Map Object to Dummy", InputManager.GetHint(KeybindID.MapEditor_Make_Dummy_Object)))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Set Map Object to Normal", InputManager.GetHint(KeybindID.MapEditor_Make_Normal_Object)))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }

            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                if (ImGui.MenuItem("Disable Map Object Game Presence", InputManager.GetHint(KeybindID.MapEditor_Disable_Game_Presence)))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Enable Map Object Game Presence", InputManager.GetHint(KeybindID.MapEditor_Enable_Game_Presence)))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
                }
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        GUI.WrappedText("Use these actions to adjust the in-game visibility of map objects.");

        if (SupportsDummyConversion())
        {
            GUI.Spacer();
            GUI.SimpleHeader("Dummy", "Toggle the dummy state type for selected map objects. Note: conversion into a dummy type will cause property data loss.");

            GUI.MultiButtonInput("dummyActions",
                "makeDummy", "Convert to Dummy Type", "Convert the selected enemies, objects and assets into their dummy types, which the game does not load.", MakeObjectDummy,
                "makeNormal", "Convert to Normal Type", "Convert the selected dummy enemies, objects and assets back into their normal types.", MakeObjectNormal);
        }

        if (Project.Descriptor.ProjectType is ProjectType.ER)
        {
            GUI.Spacer();
            GUI.SimpleHeader("Game Presence", "Toggle the game presence type for selected map objects. Game Presence can be used to hide a map object without losing property data.");

            GUI.MultiButtonInput("gamePresActions",
                "makeVisible", "Make Visible", "Set GameEditionDisable to 0 on the selected parts, so they appear in game.", EnableObjectGamePresence,
                "makeHidden", "Make Hidden", "Set GameEditionDisable to 1 on the selected parts, so they are hidden in game.", DisableObjectGamePresence);
        }
    }

    private bool SupportsDummyConversion()
    {
        return Project.Descriptor.ProjectType is not (ProjectType.DS2 or ProjectType.DS2S or ProjectType.DB);
    }

    public void MakeObjectDummy()
    {
        ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
    }

    public void MakeObjectNormal()
    {
        ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
    }

    public void EnableObjectGamePresence()
    {
        ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
    }

    public void DisableObjectGamePresence()
    {
        ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyGameVisibilityChange(GameVisibilityType targetType, GameVisibilityState targetState)
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (targetType == GameVisibilityType.GameEditionDisable)
            {
                ChangeGamePresence(targetState == GameVisibilityState.Disable ? 1u : 0u);
            }

            if (targetType == GameVisibilityType.DummyObject)
            {
                if (targetState == GameVisibilityState.Disable)
                {
                    string[] sourceTypes = { "Enemy", "Object", "Asset" };
                    string[] targetTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
                    ChangeMapObjectType(sourceTypes, targetTypes);
                }
                if (targetState == GameVisibilityState.Enable)
                {
                    string[] sourceTypes = { "DummyEnemy", "DummyObject", "DummyAsset" };
                    string[] targetTypes = { "Enemy", "Object", "Asset" };
                    ChangeMapObjectType(sourceTypes, targetTypes);
                }
            }
        }
        else
        {
            Smithbox.LogError<GameVisibilityAction>("No object selected.");
        }

        View.DelayPicking();
    }
    /// <summary>
    /// Only ER parts have the field, so regions and events in the selection are left alone
    /// </summary>
    private void ChangeGamePresence(uint value)
    {
        if (View.Project.Descriptor.ProjectType != ProjectType.ER)
            return;

        HashSet<Entity> parts = View.ViewportSelection.GetFilteredSelection<MsbEntity>()
            .Where(x => x.WrappedObject is MSBE.Part)
            .Cast<Entity>()
            .ToHashSet();

        if (parts.Count == 0)
        {
            Smithbox.LogError<GameVisibilityAction>("No part selected.");
            return;
        }

        var prop = typeof(MSBE.Part).GetProperty(nameof(MSBE.Part.GameEditionDisable));
        var action = new PropMultChangeAction(View, prop, parts, value);

        View.ViewportActionManager.ExecuteAction(action);
    }

    public void ChangeMapObjectType(string[] sourceTypes, string[] targetTypes)
    {
        Type msbclass;
        switch (View.Project.Descriptor.ProjectType)
        {
            case ProjectType.DES:
                msbclass = typeof(MSBD);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                msbclass = typeof(MSB1);
                break;
            case ProjectType.DS3:
                msbclass = typeof(MSB3);
                break;
            case ProjectType.BB:
                msbclass = typeof(MSBB);
                break;
            case ProjectType.SDT:
                msbclass = typeof(MSBS);
                break;
            case ProjectType.ER:
                msbclass = typeof(MSBE);
                break;
            case ProjectType.AC6:
                msbclass = typeof(MSB_AC6);
                break;
            case ProjectType.NR:
                msbclass = typeof(MSB_NR);
                break;
            case ProjectType.DB_NT:
                msbclass = typeof(MSB_DB_NT);
                break;
            // DS2 has no dummy types, and DB has no MSB class yet
            default:
                Smithbox.LogError<GameVisibilityAction>("Dummy conversion is not supported for this project type.");
                return;
        }
        List<MsbEntity> sourceList = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

        EntChangeTypeAction action = new(View, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);

        View.ViewportActionManager.ExecuteAction(action);
    }

}