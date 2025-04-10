using StudioCore.Editor;
using System.Collections.Generic;
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
