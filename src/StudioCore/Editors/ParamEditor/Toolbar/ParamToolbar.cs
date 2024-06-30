using ImGuiNET;
using StudioCore.Editor;
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

namespace StudioCore.Editors.ParamEditor.Toolbar
{

    public enum ParamToolbarAction
    {
        None,
        SortRows,
        ImportRowNames,
        ExportRowNames,
        TrimRowNames,
        DuplicateRow,
        MassEdit,
        MassEditScripts,
        FindRowIdInstances,
        FindValueInstances,
        UpgradeParams
    }

    public class ParamToolbar
    {
        public static ActionManager EditorActionManager;

        public static ParamToolbarAction SelectedAction;

        public static List<string> TargetTypes = new List<string>
        {
            "Selected Param",
            "All Params"
        };

        public static List<string> SourceTypes = new List<string>
        {
            "Smithbox",
            "Project"
        };

        public ParamToolbar(ActionManager actionManager)
        {
            EditorActionManager = actionManager;
        }
    }
}
