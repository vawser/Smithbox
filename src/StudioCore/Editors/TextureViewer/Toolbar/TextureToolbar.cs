using ImGuiNET;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.TextureViewer;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer.Toolbar;

public enum TextureViewerAction
{
    None,
    ExportTexture
}

public class TextureToolbar
{

    public static TextureViewerAction SelectedAction;

    public TextureToolbar()
    {

    }
}
