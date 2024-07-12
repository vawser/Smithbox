using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
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
                ImguiUtils.WrappedText($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Core_Duplicate.HintText)}");
                ImguiUtils.WrappedText("");

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
            /*
            // Unload representative model
            Smithbox.EditorHandler.ModelEditor._universe.UnloadModels(true);

            var currentFLVER = Smithbox.EditorHandler.ModelEditor.ResourceHandler.CurrentFLVER;

            // Dummy
            if(Smithbox.EditorHandler.ModelEditor.ModelHierarchy._lastSelectedEntry == ModelEntrySelectionType.Dummy)
            {
                var index = Smithbox.EditorHandler.ModelEditor.ModelHierarchy._selectedDummy;

                if(index != -1)
                {
                    var clone = currentFLVER.Dummies[index].Clone();
                    currentFLVER.Dummies.Insert(index, clone);
                }
            }

            Smithbox.EditorHandler.ModelEditor._selection.ClearSelection();
            Smithbox.EditorHandler.ModelEditor.ModelHierarchy.ResetSelection();
            //Smithbox.EditorHandler.ModelEditor.ViewportHandler._flverhandle.Get().Dispose();

            Smithbox.EditorHandler.ModelEditor.Save();

            Thread.Sleep(1000);

            Smithbox.EditorHandler.ModelEditor.ModelSelectionView.ReloadModel();
            */
        }
    }
}
