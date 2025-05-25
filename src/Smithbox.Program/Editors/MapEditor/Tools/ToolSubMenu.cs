using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Tools.PatrolRouteDraw;
using StudioCore.Editors.MapEditor.Tools.WorldMap;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Framework.MapActionHandler;

namespace StudioCore.Editors.MapEditor.Tools;

public class ToolSubMenu
{
    private MapEditorScreen Editor;
    private MapActionHandler Handler;

    private bool PatrolsVisualised = false;

    public ToolSubMenu(MapEditorScreen screen, MapActionHandler handler)
    {
        Editor = screen;
        Handler = handler;
    }

    public void Shortcuts()
    {
        /// Toggle Patrol Route Visualisation
        if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_TogglePatrolRouteRendering))
            {
                if (!PatrolsVisualised)
                {
                    PatrolsVisualised = true;
                    PatrolDrawManager.Generate(Editor);
                }
                else
                {
                    PatrolDrawManager.Clear();
                    PatrolsVisualised = false;
                }
            }
        }

        Editor.RotationIncrement.Shortcuts();
        Editor.KeyboardMovement.Shortcuts();

        //Selection Groups
        Editor.SelectionGroupView.SelectionGroupShortcuts();
    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("World Map", KeyBindings.Current.MAP_ToggleWorldMap.HintText))
            {
                Editor.WorldMapView.DisplayMenuOption();
            }
            UIHelper.Tooltip($"Open the world map for Elden Ring.\nAllows you to easily select open-world tiles.");

            ///--------------------
            /// Color Picker
            ///--------------------
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            ImGui.Separator();

            ///--------------------
            /// Toggle Editor Visibility by Tag
            ///--------------------
            if (ImGui.BeginMenu("Toggle Editor Visibility by Tag"))
            {
                ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                UIHelper.Tooltip("Specific which tag the map objects will be filtered by.");

                if (ImGui.MenuItem("Enable Visibility"))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = true;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = false;

                    Handler.ApplyEditorVisibilityChangeByTag();
                }
                if (ImGui.MenuItem("Disable Visibility"))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = true;

                    Handler.ApplyEditorVisibilityChangeByTag();
                }

                ImGui.EndMenu();
            }

            ///--------------------
            /// Patrol Route Visualisation
            ///--------------------
            if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
            {
                if (ImGui.BeginMenu("Patrol Route Visualisation"))
                {
                    if (ImGui.MenuItem("Display"))
                    {
                        PatrolDrawManager.Generate(Editor);
                    }
                    if (ImGui.MenuItem("Clear"))
                    {
                        PatrolDrawManager.Clear();
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Generate Navigation Data
            ///--------------------
            if (Editor.Project.ProjectType is ProjectType.DES || Editor.Project.ProjectType is ProjectType.DS1 || Editor.Project.ProjectType is ProjectType.DS1R)
            {
                if (ImGui.BeginMenu("Navigation Data"))
                {
                    if (ImGui.MenuItem("Generate"))
                    {
                        Handler.GenerateNavigationData();
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Entity ID Checker
            ///--------------------
            if (Editor.Project.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
            {
                if (ImGui.BeginMenu("Entity ID Checker"))
                {
                    if (Editor.IsAnyMapLoaded())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                        {
                            foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
                            {
                                var mapID = entry.Key.Filename;
                                var container = entry.Value.MapContainer;

                                if (container != null)
                                {
                                    if (ImGui.Selectable(mapID))
                                    {
                                        Handler._targetMap = (mapID, container);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.MenuItem("Check"))
                        {
                            Handler.ApplyEntityChecker();
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            ///--------------------
            /// Name Map Objects
            ///--------------------
            // Tool for AC6 since its maps come with unnamed Regions and Events
            if (Editor.Project.ProjectType is ProjectType.AC6)
            {
                if (ImGui.BeginMenu("Rename Map Objects"))
                {
                    if (Editor.IsAnyMapLoaded())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                        {
                            foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
                            {
                                var mapID = entry.Key.Filename;
                                var container = entry.Value.MapContainer;

                                if (container != null)
                                {
                                    if (ImGui.Selectable(mapID))
                                    {
                                        Handler._targetMap = (mapID, container);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.MenuItem("Apply Japanese Names"))
                        {
                            DialogResult result = PlatformUtils.Instance.MessageBox(
                            $"This will apply the developer map object names (in Japanese) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object", 
                            "Warning",
                            MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                Handler.ApplyMapObjectNames(true);
                            }
                        }

                        if (ImGui.MenuItem("Apply English Names"))
                        {
                            DialogResult result = PlatformUtils.Instance.MessageBox(
                            $"This will apply the developer map object names (in machine translated English) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object",
                            "Warning",
                            MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                Handler.ApplyMapObjectNames(false);
                            }
                        }
                    }

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip("Applies descriptive name for map objects from developer name list.");
            }

            ImGui.EndMenu();
        }
    }
}

