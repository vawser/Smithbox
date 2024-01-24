using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Settings;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.AnimationEditor;

public class AnimationEditorScreen : EditorScreen
{
    private readonly PropertyEditor _propEditor;
    private ProjectSettings _projectSettings;

    public readonly AssetLocator AssetLocator;

    public ActionManager EditorActionManager = new();

    public AnimationEditorScreen(Sdl2Window window, GraphicsDevice device, AssetLocator locator)
    {
        AssetLocator = locator;
        _propEditor = new PropertyEditor(EditorActionManager);
    }

    public string EditorName => "Animation Editor";
    public string CommandEndpoint => "animation";
    public string SaveType => "Animation";

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
