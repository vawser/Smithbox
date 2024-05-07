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
                ImguiUtils.WrappedText("Delete selected FLVER property.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedText("WARNING: there are no safeguards ensuring that the model will still load correctly in-game, use this action with caution.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Mesh:");
                ImGui.Checkbox("Affect Face Sets Only##facesetsOnly", ref CFG.Current.ModelEditor_Toolbar_DeleteProperty_FaceSetsOnly);
                ImguiUtils.ShowHoverTooltip("Deleting mesh properties will clear the Face Sets instead of deleting the mesh itself.\n\nThis is required if you want to retain any Havok Physics setup already associated with a mesh.");
                ImguiUtils.WrappedText("");
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

            FlverResource r = ModelEditorScreen._flverhandle.Get();

            foreach (var curSel in sel.GetSelection())
            {
                Entity selected = curSel as Entity;
                ModelSceneTree.Model.DeleteMeshIfValid(selected, r);
                ModelSceneTree.Model.DeleteMaterialIfValid(selected, r);
                ModelSceneTree.Model.DeleteLayoutIfValid(selected, r);
                ModelSceneTree.Model.DeleteBoneIfValid(selected, r);
                ModelSceneTree.Model.DeleteDummyPolyIfValid(selected, r);
            }

            ModelToolbar._screen.Save();

            CFG.Current.ModelEditor_RenderingUpdate = true;
        }
    }
}
