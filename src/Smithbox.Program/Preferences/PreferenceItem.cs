using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class PreferenceItem
{
    public PreferenceCategory Category { get; set; }

    public List<ProjectType> DisplayRestrictions = new(){
        ProjectType.Undefined
    };

    public string Section;

    public string Title;
    public string Description;

    public bool Spacer = false;
    public bool InlineName = true;

    public Action PreDraw;
    public Action Draw;
    public Action PostDraw;
}

public enum PreferenceCategory
{
    System,
    Project,
    Interface,
    Viewport,
    MapEditor,
    ModelEditor,
    ParamEditor,
    TextEditor,
    GparamEditor,
    MaterialEditor,
    TextureViewer
}