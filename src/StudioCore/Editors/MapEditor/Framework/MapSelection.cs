using StudioCore.Core;
using StudioCore.Editors.MapEditor.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapSelection
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public string SelectedMapID;
    public MapContentView SelectedMapView;

    public MapSelection(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
}
