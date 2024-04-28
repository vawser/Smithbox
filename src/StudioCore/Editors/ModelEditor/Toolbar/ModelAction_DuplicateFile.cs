using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextEditor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Toolbar
{
    public class ModelAction_DuplicateFile
    {
        public static void Setup()
        {

        }

        public static void Select()
        {
            if (ImGui.RadioButton("Duplicate Asset##tool_DuplicateFile", ModelToolbar.SelectedAction == ModelEditorAction.DuplicateFile))
            {
                ModelToolbar.SelectedAction = ModelEditorAction.DuplicateFile;
            }
            ImguiUtils.ShowHoverTooltip("Duplicate and rename the current asset selected in the Asset Browser.");
        }

        public static void Configure()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DuplicateFile)
            {
                ImGui.Text("Duplicate and rename the current asset selected in the Asset Browser.");
                ImGui.Text("");

                ImGui.Text("New ID:");
                ImGui.InputText("##newId", ref CFG.Current.ModelEditor_Toolbar_DuplicateFile_NewName, 255);
                ImguiUtils.ShowHoverTooltip("Model ID with which to rename the duplicated asset.");
                ImGui.Text("");
            }
        }

        public static void Act()
        {
            if (ModelToolbar.SelectedAction == ModelEditorAction.DuplicateFile)
            {
                if (ImGui.Button("Apply##action_Selection_DuplicateFile", new Vector2(200, 32)))
                {
                    DuplicateSelectedAsset();
                }
            }
        }

        public static void DuplicateSelectedAsset()
        {
            switch(Project.Type)
            {
                case ProjectType.AC6:
                    DuplicateAsset_AC6();
                    break;
                case ProjectType.ER:
                    DuplicateAsset_ER();
                    break;
                case ProjectType.DS3:
                    DuplicateAsset_DS3();
                    break;
                default:
                    PlatformUtils.Instance.MessageBox("This asset cannot be duplicated by this action.", "Smithbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        public static void DuplicateAsset_AC6()
        {
            //*** Chr

            // anibnd

            // behbnd

            // chrbnd

            // texbnd

            //*** AEG


            //*** Part

            //*** MapPiece
        }

        public static void DuplicateAsset_ER()
        {
            //*** Chr

            // anibnd

            // behbnd

            // chrbnd

            // texbnd

            //*** AEG


            //*** Part

            //*** MapPiece
        }

        public static void DuplicateAsset_DS3()
        {
            //*** Chr

            // anibnd

            // behbnd

            // chrbnd

            // texbnd

            //*** Obj


            //*** Part


            //*** MapPiece
        }
    }
}
