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

namespace StudioCore.MaterialEditor;

public class MaterialEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public ActionManager EditorActionManager = new();

    public MaterialEditorScreen(Sdl2Window window, GraphicsDevice device)
    {
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Material Editor";
    public string CommandEndpoint => "material";
    public string SaveType => "Material";

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
