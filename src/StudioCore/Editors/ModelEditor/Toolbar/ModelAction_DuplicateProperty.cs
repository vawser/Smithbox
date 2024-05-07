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
    public static class ModelAction_DuplicateProperty
    {
        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.RadioButton("Duplicate Property##tool_DuplicateProperty", ModelToolbar.SelectedAction == ModelEditorAction.DuplicateProperty))
            {
                ModelToolbar.SelectedAction = ModelEditorAction.DuplicateProperty;
            }
            ImguiUtils.ShowHoverTooltip("Duplicate selected FLVER property.");
        }

        public static void Configure()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DuplicateProperty)
            {
                ImguiUtils.WrappedText("Duplicate selected FLVER property.");
                ImguiUtils.WrappedText("");
                ImguiUtils.WrappedText("WARNING: there are no safeguards ensuring that the model will still load correctly in-game, use this action with caution.");
                ImguiUtils.WrappedText("");

                ImguiUtils.WrappedText("Amount:");
                ImGui.InputInt("##amount", ref CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount);
                ImguiUtils.ShowHoverTooltip("Number of times to duplicate the selected property.");
                ImguiUtils.WrappedText("");
            }
        }

        public static void Act()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DuplicateProperty)
            {
                if (ImGui.Button("Apply##action_Selection_DuplicateProperty", new Vector2(200, 32)))
                {
                    DuplicateFLVERProperty();
                }
            }
        }

        public static void DuplicateFLVERProperty()
        {
            CFG.Current.ModelEditor_RenderingUpdate = false;

            ViewportSelection sel = ModelEditorScreen._sceneTree.GetCurrentSelection();

            if (sel.GetSelection().Count < 1)
            {
                return;
            }

            FlverResource r = ModelEditorScreen._flverhandle.Get();

            foreach(var curSel in sel.GetSelection())
            {
                Entity selected = curSel as Entity;
                ModelSceneTree.Model.DuplicateMeshIfValid(selected, r);
                ModelSceneTree.Model.DuplicateMaterialIfValid(selected, r);
                ModelSceneTree.Model.DuplicateLayoutIfValid(selected, r);
                ModelSceneTree.Model.DuplicateBoneIfValid(selected, r);
                ModelSceneTree.Model.DuplicateDummyPolyIfValid(selected, r);
            }

            ModelToolbar._screen.Save();

            CFG.Current.ModelEditor_RenderingUpdate = true;
        }
    }
}
