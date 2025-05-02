using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using StudioCore.Tools;
using System.Linq;
using System.Windows.Forms;

namespace StudioCore.Editors.MapEditorNS;

public class ToolSubMenu
{
    private MapEditor Editor;

    private bool PatrolsVisualised = false;

    public ToolSubMenu(MapEditor editor)
    {
        Editor = editor;
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
                    Editor.PatrolDrawManager.Generate();
                }
                else
                {
                    Editor.PatrolDrawManager.Clear();
                    PatrolsVisualised = false;
                }
            }
        }

        Editor.RotationIncrement.Shortcuts();
        Editor.KeyboardMovement.Shortcuts();

        //Selection Groups
        Editor.SelectionGroupView.SelectionGroupShortcuts();
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

                    Editor.ActionHandler.ApplyEditorVisibilityChangeByTag();
                }
                if (ImGui.MenuItem("Disable Visibility"))
                {
                    CFG.Current.Toolbar_Tag_Visibility_State_Enabled = false;
                    CFG.Current.Toolbar_Tag_Visibility_State_Disabled = true;

                    Editor.ActionHandler.ApplyEditorVisibilityChangeByTag();
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
                        Editor.PatrolDrawManager.Generate();
                    }
                    if (ImGui.MenuItem("Clear"))
                    {
                        Editor.PatrolDrawManager.Clear();
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
                        Editor.ActionHandler.GenerateNavigationData();
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
                    if (Editor.Universe.LoadedObjectContainers != null && Editor.Universe.LoadedObjectContainers.Any())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Editor.ActionHandler._targetMap.Item1))
                        {
                            foreach (var obj in Editor.Universe.LoadedObjectContainers)
                            {
                                if (obj.Value != null)
                                {
                                    if (ImGui.Selectable(obj.Key))
                                    {
                                        Editor.ActionHandler._targetMap = (obj.Key, obj.Value);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.MenuItem("Check"))
                        {
                            Editor.ActionHandler.ApplyEntityChecker();
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
                    if (Editor.Universe.LoadedObjectContainers != null && Editor.Universe.LoadedObjectContainers.Any())
                    {
                        if (ImGui.BeginCombo("##Targeted Map", Editor.ActionHandler._targetMap.Item1))
                        {
                            foreach (var obj in Editor.Universe.LoadedObjectContainers)
                            {
                                if (obj.Value != null)
                                {
                                    if (ImGui.Selectable(obj.Key))
                                    {
                                        Editor.ActionHandler._targetMap = (obj.Key, obj.Value);
                                        break;
                                    }
                                }
                            }
                            ImGui.EndCombo();
                        }

                        if (ImGui.MenuItem("Apply Japanese Names"))
                        {
                            DialogResult result = MessageBox.Show(
                            $"This will apply the developer map object names (in Japanese) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object", 
                            "Warning",
                            MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                Editor.ActionHandler.ApplyMapObjectNames(true);
                            }
                        }

                        if (ImGui.MenuItem("Apply English Names"))
                        {
                            DialogResult result = MessageBox.Show(
                            $"This will apply the developer map object names (in machine translated English) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object",
                            "Warning",
                            MessageBoxButtons.YesNo);

                            if (result == DialogResult.Yes)
                            {
                                Editor.ActionHandler.ApplyMapObjectNames(false);
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

