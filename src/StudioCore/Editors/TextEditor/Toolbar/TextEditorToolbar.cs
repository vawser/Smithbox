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
        SyncWeaponDescription
    }

    public class TextEditorToolbar
    {
        public static TextEditorAction SelectedAction;

        public TextEditorToolbar() 
        {
            TextAction_SearchAndReplace.Setup();
        }

        public void ShowActionList()
        {
            TextAction_SearchAndReplace.Select();
            TextAction_SyncWeaponDescriptions.Select();
        }

        public void ShowActionConfiguration()
        {
            TextAction_SearchAndReplace.Configure();
            TextAction_SyncWeaponDescriptions.Configure();
        }
    }
}
