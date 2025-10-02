using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class GameVisibilityAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public GameVisibilityAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeDummyObject) && 
            Editor.ViewportSelection.IsSelection())
        {
            ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_MakeNormalObject) && 
            Editor.ViewportSelection.IsSelection())
        {
            ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_DisableGamePresence) && 
            Editor.ViewportSelection.IsSelection())
        {
            ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
        }
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_EnableGamePresence) && 
            Editor.ViewportSelection.IsSelection())
        {
            ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Enable);
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        if (ImGui.BeginMenu("Game Visibility"))
        {
            if (ImGui.MenuItem("Make Selected Normal Object into Dummy Object", KeyBindings.Current.MAP_MakeDummyObject.HintText))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Make Selected Dummy Object into Normal Object", KeyBindings.Current.MAP_MakeNormalObject.HintText))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }

            if (Project.ProjectType is ProjectType.ER)
            {
                if (ImGui.MenuItem("Disable Game Presence of Selected", KeyBindings.Current.MAP_DisableGamePresence.HintText))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Enable Game Presence of Selected", KeyBindings.Current.MAP_EnableGamePresence.HintText))
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
            if (ImGui.MenuItem("Make Selected Normal Object into Dummy Object", KeyBindings.Current.MAP_MakeDummyObject.HintText))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Disable);
            }

            if (ImGui.MenuItem("Make Selected Dummy Object into Normal Object", KeyBindings.Current.MAP_MakeNormalObject.HintText))
            {
                ApplyGameVisibilityChange(GameVisibilityType.DummyObject, GameVisibilityState.Enable);
            }

            if (Project.ProjectType is ProjectType.ER)
            {
                if (ImGui.MenuItem("Disable Game Presence of Selected", KeyBindings.Current.MAP_DisableGamePresence.HintText))
                {
                    ApplyGameVisibilityChange(GameVisibilityType.GameEditionDisable, GameVisibilityState.Disable);
                }

                if (ImGui.MenuItem("Enable Game Presence of Selected", KeyBindings.Current.MAP_EnableGamePresence.HintText))
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
        if (Editor.ViewportSelection.IsSelection())
        {
            if (targetType == GameVisibilityType.GameEditionDisable)
            {
                if (targetState == GameVisibilityState.Disable)
                {
                    List<MsbEntity> sourceList = Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();
                    foreach (MsbEntity s in sourceList)
                    {
                        if (Editor.Project.ProjectType == ProjectType.ER)
                        {
                            s.SetPropertyValue("GameEditionDisable", 1);
                        }
                    }
                }
                if (targetState == GameVisibilityState.Enable)
                {
                    List<MsbEntity> sourceList = Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();
                    foreach (MsbEntity s in sourceList)
                    {
                        if (Editor.Project.ProjectType == ProjectType.ER)
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
        switch (Editor.Project.ProjectType)
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
            case ProjectType.ACFA:
                msbclass = typeof(MSBFA);
                break;
            case ProjectType.ACV:
                msbclass = typeof(MSBV);
                break;
            case ProjectType.ACVD:
                msbclass = typeof(MSBVD);
                break;
            default:
                throw new ArgumentException("type must be valid");
        }
        List<MsbEntity> sourceList = Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList();

        ChangeMapObjectType action = new(Editor, msbclass, sourceList, sourceTypes, targetTypes, "Part", true);
        Editor.EditorActionManager.ExecuteAction(action);
    }

}