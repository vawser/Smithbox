using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editor;
using System.Reflection;

namespace StudioCore.MaterialEditor;

public class PropertyEditor
{
    private Dictionary<string, PropertyInfo[]> _propCache = new();

    public ActionManager ContextActionManager;

    public PropertyEditor(ActionManager manager)
    {
        ContextActionManager = manager;
    }
}
