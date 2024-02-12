using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.TextureViewer;

public class TextureViewerScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    public TextureViewerScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Texture Viewer##TextureViewerEditor";
    public string CommandEndpoint => "texture";
    public string SaveType => "Texture";

    public void DrawEditorMenu()
    {
    }

    public void OnGUI(string[] initcmd)
    {
        
    }

    public void OnProjectChanged(ProjectSettings newSettings)
    {
        _projectSettings = newSettings;

        ResetActionManager();
    }

    public void Save()
    {

    }

    public void SaveAll()
    {

    }

    private void ResetActionManager()
    {
        EditorActionManager.Clear();
    }
}
