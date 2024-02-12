using ImGuiNET;
using Silk.NET.SDL;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor
{
    public class SavedSelectionToolbar
    {
        private readonly EntityActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly MapSelection _selection;

        private Universe _universe;

        private IViewport _viewport;

        public SavedSelectionToolbar(RenderScene scene, MapSelection sel, EntityActionManager manager, Universe universe, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _viewport = viewport;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Selections##SelectionToolbar"))
            {

            }

            ImGui.End();
        }
    }
}
