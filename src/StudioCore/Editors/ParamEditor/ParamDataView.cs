using ImGuiNET;
using StudioCore.Editor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor
{
    public class ParamDataView
    {
        private ActionManager EditorActionManager;

        public ParamDataView(ActionManager actionManager)
        {
            EditorActionManager = actionManager;
        }

        public void OnGui()
        {
            if (Project.Type == ProjectType.Undefined)
                return;

            ImGui.Text("Test");
        }
    }
}
