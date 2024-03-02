using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Gui;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static StudioCore.TextEditor.TextEditorScreen;

namespace StudioCore.Editors.TextEditor.Toolbar
{
    public enum TextEditorAction
    {
        None,
        SearchAndReplace,
        SyncEntries
    }

    public class TextEditorToolbar
    {
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

        public TextEditorToolbar() 
        {
            TextAction_SearchAndReplace.Setup();
            TextAction_SyncEntries.Setup();
        }

        public void ShowActionList()
        {
            TextAction_SearchAndReplace.Select();
            TextAction_SyncEntries.Select();
        }

        public void ShowActionConfiguration()
        {
            TextAction_SearchAndReplace.Configure();
            TextAction_SyncEntries.Configure();
        }
    }
}
