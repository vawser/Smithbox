using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Toolbar;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor
{
    public enum SelectedTool
    {
        None,

        // Selection
        Selection_Go_to_in_Object_List,
        Selection_Frame_in_Viewport,
        Selection_Move_to_Camera,
        Selection_Move_to_Grid,
        Selection_Toggle_Presence,
        Selection_Toggle_Visibility,
        Selection_Create,
        Selection_Duplicate,
        Selection_Rotate,
        Selection_Scramble,
        Selection_Replicate,

        // Global
        Selection_Assign_Entity_Group_ID,
        Selection_Toggle_Object_Visibility_by_Tag,
        Selection_Render_Patrol_Routes,
        Selection_Check_for_Errors,
        Selection_Generate_Navigation_Data
    }

    public static class MapEditorState
    {

        public static SelectedTool CurrentTool;

        public static IEnumerable<ObjectContainer> LoadedMaps;

        public static Universe Universe;

        public static RenderScene Scene;

        public static ViewportActionManager ActionManager;

        public static IViewport Viewport;

        public static MapEditorToolbar Toolbar;
    }
}
