using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.MapEditor;

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

            Editor.EditorVisibilityAction.OnToolMenu();

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
                Editor.EntityIdCheckAction.OnToolMenu();
            }

            ///--------------------
            /// Name Map Objects
            ///--------------------
            // Tool for AC6 since its maps come with unnamed Regions and Events
            if (Editor.Project.ProjectType is ProjectType.AC6)
            {
                Editor.EntityRenameAction.OnToolMenu();
            }

            ImGui.EndMenu();
        }
    }
}

