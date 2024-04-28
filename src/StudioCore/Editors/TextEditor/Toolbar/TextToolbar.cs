using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.TextEditor.TextEditorScreen;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public enum TextEditorAction
    {
        None,
        Duplicate,
        Delete,
        SearchAndReplace,
        SyncEntries
    }

    public class TextToolbar
    {
        public static ActionManager EditorActionManager;

        public static TextEditorAction SelectedAction;

        public static List<string> TargetTypes = new List<string>
        {
            "Selected Category",
            "Selected Entry"
        };

        public static List<string> TextCategories = new List<string>
        {
            "Title",
            "Description",
            "Summary",
            "Text Body",
            "Extra Text",
            "All"
        };

        public TextToolbar(ActionManager actionManager)
        {
            EditorActionManager = actionManager;

            TextAction_SearchAndReplace.Setup();
            TextAction_SyncEntries.Setup();
            TextAction_Duplicate.Setup();
            TextAction_Delete.Setup();
        }
    }
}
