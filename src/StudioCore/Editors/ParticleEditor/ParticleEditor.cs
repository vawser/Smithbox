using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System.Reflection;
using StudioCore.Editors;

namespace StudioCore.ParticleEditor;

public class PropertyEditor
{
    private Dictionary<string, PropertyInfo[]> _propCache = new();

    public ActionManager ContextActionManager;

    public FXR3 currentParticle;

    public PropertyEditor(ActionManager manager)
    {
        ContextActionManager = manager;
    }

    public void OnGui(FXR3 particle)
    {
        currentParticle = particle;



    }
}

