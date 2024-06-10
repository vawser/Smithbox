using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System.Reflection;

namespace StudioCore.MergeTool;

public class MergePropertyEditor
{
    private Dictionary<string, PropertyInfo[]> _propCache = new();

    public ActionManager ContextActionManager;

    public MergePropertyEditor(ActionManager manager)
    {
        ContextActionManager = manager;
    }
}
