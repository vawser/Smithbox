using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Editors.TextEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Toolbar
{
    public enum ModelEditorAction
    {
        None,
        DuplicateFile,
        DuplicateProperty,
        DeleteProperty
    }

    public class ModelToolbar
    {
        public static ModelEditorScreen _screen;

        public static ViewportActionManager EditorActionManager;

        public static ModelEditorAction SelectedAction;

        public ModelToolbar(ViewportActionManager actionManager, ModelEditorScreen screen)
        {
            EditorActionManager = actionManager;
            _screen = screen;
        }
    }
}
