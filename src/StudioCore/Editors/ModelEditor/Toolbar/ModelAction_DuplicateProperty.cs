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
            if (ImGui.RadioButton("Duplicate Selected Property##tool_DuplicateProperty", ModelToolbar.SelectedAction == ModelEditorAction.DuplicateProperty))
            {
                ModelToolbar.SelectedAction = ModelEditorAction.DuplicateProperty;
            }
            ImguiUtils.ShowHoverTooltip("Duplicate selected FLVER property.");
        }

        public static void Configure()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DuplicateProperty)
            {
                ImGui.Text("Duplicate selected FLVER property.");
                ImGui.Text("");

                ImGui.Text("Amount:");
                ImGui.InputInt("##amount", ref CFG.Current.ModelEditor_Toolbar_DuplicateProperty_Amount);
                ImguiUtils.ShowHoverTooltip("Number of times to duplicate the selected property.");
                ImGui.Text("");
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
            ViewportSelection sel = ModelEditorScreen._sceneTree.GetCurrentSelection();

            if (sel.GetSelection().Count < 1)
            {
                return;
            }

            ISelectable first = sel.GetSelection().First();
            Entity selected = first as Entity;

            FlverResource r = ModelEditorScreen._flverhandle.Get();

            ModelSceneTree.Model.DuplicateMeshIfValid(selected, r);

            CFG.Current.ModelEditor_RenderingUpdate = true;
        }
    }
}
