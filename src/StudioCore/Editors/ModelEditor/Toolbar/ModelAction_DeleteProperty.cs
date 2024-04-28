using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Toolbar
{
    public static class ModelAction_DeleteProperty
    {
        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.RadioButton("Delete Property##tool_DeleteProperty", ModelToolbar.SelectedAction == ModelEditorAction.DeleteProperty))
            {
                ModelToolbar.SelectedAction = ModelEditorAction.DeleteProperty;
            }
            ImguiUtils.ShowHoverTooltip("Delete selected FLVER property.");
        }

        public static void Configure()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DeleteProperty)
            {
                ImGui.Text("Delete selected FLVER property.");
                ImGui.Text("");
            }
        }

        public static void Act()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DeleteProperty)
            {
                if (ImGui.Button("Apply##action_Selection_DeleteProperty", new Vector2(200, 32)))
                {
                    DeleteFLVERProperty();
                }
            }
        }

        public static void DeleteFLVERProperty()
        {
            ViewportSelection sel = ModelEditorScreen._sceneTree.GetCurrentSelection();

            if (sel.GetSelection().Count < 1)
            {
                return;
            }

            ISelectable first = sel.GetSelection().First();
            Entity selected = first as Entity;

            FlverResource r = ModelEditorScreen._flverhandle.Get();

            ModelSceneTree.Model.DeleteMeshIfValid(selected, r);

            CFG.Current.ModelEditor_RenderingUpdate = true;
        }
    }
}
