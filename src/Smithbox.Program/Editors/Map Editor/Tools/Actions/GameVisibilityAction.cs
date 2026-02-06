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
        var windowWidth = ImGui.GetWindowWidth();

        // Not shown here
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
                if (targetState == GameVisibilityState.Disable)
                {
                    List<MsbEntity> sourceList = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();
                    foreach (MsbEntity s in sourceList)
                    {
                        if (View.Project.Descriptor.ProjectType == ProjectType.ER)
                        {
                            s.SetPropertyValue("GameEditionDisable", 1);
                        }
                    }
                }
                if (targetState == GameVisibilityState.Enable)
                {
                    List<MsbEntity> sourceList = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();
                    foreach (MsbEntity s in sourceList)
                    {
                        if (View.Project.Descriptor.ProjectType == ProjectType.ER)
                        {
                            s.SetPropertyValue("GameEditionDisable", 0);
                        }
                    }
                }
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
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
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
            case ProjectType.DS2:
            case ProjectType.DS2S:
                msbclass = typeof(MSB2);
                //break;
                return; //idk how ds2 dummies should work
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
            default:
                throw new ArgumentException("type must be valid");
        }
        List<MsbEntity> sourceList = View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

        ChangeMapObjectType action = new(View, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);

        View.ViewportActionManager.ExecuteAction(action);
    }

}