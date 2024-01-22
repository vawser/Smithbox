using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Aliases;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Gui;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using StudioCore.Settings;
using StudioCore.Utilities;
using Veldrid;
using System.IO;
using System;
using System.Threading;
using static SoulsFormats.ACB;

namespace StudioCore.MsbEditor
{
    public class MsbMenubar
    {
        private readonly ActionManager _actionManager;

        private readonly RenderScene _scene;
        private readonly Selection _selection;

        private AssetLocator _assetLocator;
        private MsbEditorScreen _msbEditor;

        private IViewport _viewport;

        public MsbMenubar(RenderScene scene, Selection sel, ActionManager manager, AssetLocator locator, MsbEditorScreen editor, IViewport viewport)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;

            _assetLocator = locator;
            _msbEditor = editor;
            _viewport = viewport;
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (_assetLocator.Type == GameType.Undefined)
                return;

            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($@"Toolbar##MsbMenubar"))
            {
                if (ImGui.Button("Controls##ViewportControls"))
                {
                    ImGui.OpenPopup("Viewport Controls");
                }
                if (ImGui.BeginPopup("Viewport Controls"))
                {
                    ImGui.Text($"Hold right click in viewport to activate camera mode");
                    ImGui.Text($"Forward: {KeyBindings.Current.Viewport_Cam_Forward.HintText}\n" +
                        $"Left: {KeyBindings.Current.Viewport_Cam_Left.HintText}\n" +
                        $"Back: {KeyBindings.Current.Viewport_Cam_Back.HintText}\n" +
                        $"Right: {KeyBindings.Current.Viewport_Cam_Right.HintText}\n" +
                        $"Up: {KeyBindings.Current.Viewport_Cam_Up.HintText}\n" +
                        $"Down: {KeyBindings.Current.Viewport_Cam_Down.HintText}\n" +
                        $"Fast cam: Shift\n" +
                        $"Slow cam: Ctrl\n" +
                        $"Tweak speed: Mouse wheel");
                    ImGui.EndPopup();
                }
            }
            ImGui.End();
        }
    }
}
