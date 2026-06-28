using DotNext.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.MetadataEditor;

public class MetadataSelection
{
    public ProjectMetadataScreen Editor;

    public MetadataEditorMode EditorMode { get; set; } = MetadataEditorMode.Project;

    public MetadataSelection(ProjectMetadataScreen editor)
    {
        Editor = editor;
    }
}
