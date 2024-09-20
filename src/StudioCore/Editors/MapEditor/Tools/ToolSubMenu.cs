using ImGuiNET;
using StudioCore.Configuration;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor.Actions;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Tools;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.Editors.MapEditor.Actions.ActionHandler;

namespace StudioCore.Editors.MapEditor.Tools;

public class ToolSubMenu
{
    private MapEditorScreen Screen;
    private ActionHandler Handler;

    private bool PatrolsVisualised = false;

    public ToolSubMenu(MapEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    public void Shortcuts()
    {
        /// Toggle Patrol Route Visualisation
        if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
        {
            if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_TogglePatrolRouteRendering))
            {
                if (!PatrolsVisualised)
                {
                    PatrolsVisualised = true;
                    PatrolDrawManager.Generate(Screen.Universe);
                }
                else
                {
                    PatrolDrawManager.Clear();
                    PatrolsVisualised = false;
                }
            }
        }

        RotationIncrement.Shortcuts();
        KeyboardMovement.Shortcuts();

        //Selection Groups
        Screen.SelectionGroupEditor.SelectionGroupShortcuts();
    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            ///--------------------
            /// Color Picker
            ///--------------------
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Color Picker", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            ///--------------------
            /// Toggle Editor Visibility by Tag
            ///--------------------
            UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.BeginMenu("Toggle Editor Visibility by Tag"))
            {
                ImGui.InputText("##targetTag", ref CFG.Current.Toolbar_Tag_Visibility_Target, 255);
                UIHelper.ShowHoverTooltip("Specific which tag the map objects will be filtered by.");

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
            if (Smithbox.ProjectType != ProjectType.DS2S && Smithbox.ProjectType != ProjectType.DS2)
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                if (ImGui.BeginMenu("Patrol Route Visualisation"))
                {
                    if (ImGui.MenuItem("Display"))
                    {
                        PatrolDrawManager.Generate(Screen.Universe);
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
            if (Smithbox.ProjectType is ProjectType.DES || Smithbox.ProjectType is ProjectType.DS1 || Smithbox.ProjectType is ProjectType.DS1R)
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
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
            if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                if (ImGui.BeginMenu("Entity ID Checker"))
                {
                    if (Screen.Universe.LoadedObjectContainers != null && Screen.Universe.LoadedObjectContainers.Any())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                        {
                            foreach (var obj in Screen.Universe.LoadedObjectContainers)
                            {
                                if (obj.Value != null)
                                {
                                    if (ImGui.Selectable(obj.Key))
                                    {
                                        Handler._targetMap = (obj.Key, obj.Value);
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
            if (Smithbox.ProjectType is ProjectType.AC6)
            {
                UIHelper.ShowMenuIcon($"{ForkAwesome.Bars}");
                if (ImGui.BeginMenu("Rename Map Objects"))
                {
                    if (Screen.Universe.LoadedObjectContainers != null && Screen.Universe.LoadedObjectContainers.Any())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Handler._targetMap.Item1))
                        {
                            foreach (var obj in Screen.Universe.LoadedObjectContainers)
                            {
                                if (obj.Value != null)
                                {
                                    if (ImGui.Selectable(obj.Key))
                                    {
                                        Handler._targetMap = (obj.Key, obj.Value);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.MenuItem("Apply Names"))
                        {
                            Handler.ApplyMapObjectNames();
                        }
                    }

                    ImGui.EndMenu();
                }
                UIHelper.ShowHoverTooltip("Applies descriptive name (based on the map object class) to map objects with blank names by default.");
            }

            ImGui.EndMenu();
        }
    }
}

