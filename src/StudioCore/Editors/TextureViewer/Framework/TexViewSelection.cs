using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.TextureViewer.Data;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Formats.JSON;
using StudioCore.Resource;
using StudioCore.Resource.Types;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexViewSelection
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    private Task LoadingTask;

    // Texture Viewer
    public FileDictionaryEntry SelectedFileEntry;
    public BinderContents SelectedBinder;

    public TextureResource ViewerTextureResource;
    public TextureResource PreviewTextureResource;

    public string SelectedTpfKey = "";
    public BinderFile SelectedTpfBinderFile = null;
    public TPF SelectedTpf;

    public string SelectedTextureKey = "";
    public TPF.Texture SelectedTexture;

    public bool SelectFile = false;
    public bool SelectTpf = false;
    public bool SelectTexture = false;

    // Texture Viewport
    public Vector2 TextureViewWindowPosition = new Vector2(0, 0);
    public Vector2 TextureViewScrollPosition = new Vector2(0, 0);

    public SubTexture PreviewSubTexture;

    public TexViewSelection(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void ResetSelection()
    {
        if (ViewerTextureResource != null)
        {
            ViewerTextureResource.Dispose();
        }
        ViewerTextureResource = null;

        if (PreviewTextureResource != null)
        {
            PreviewTextureResource.Dispose();
        }
        PreviewTextureResource = null;

        SelectedFileEntry = null;
        SelectedBinder = null;

        SelectedTpfKey = "";
        SelectedTpfBinderFile = null;
        SelectedTpf = null;

        SelectedTextureKey = "";
        SelectedTexture = null;

        PreviewSubTexture = null;
    }

    /// <summary>
    /// Load passed texture container
    /// </summary>
    /// <param name="info"></param>
    public void SelectTextureFile(FileDictionaryEntry fileEntry, BinderContents binder)
    {
        SelectedFileEntry = fileEntry;
        SelectedBinder = binder;
    }

    public void SelectTpfFile(BinderFile binderFile, TPF file)
    {
        SelectedTpfKey = binderFile.Name;
        SelectedTpfBinderFile = binderFile;
        SelectedTpf = file;
    }

    public void SelectTextureEntry(string textureName, TPF.Texture texture)
    {
        SelectedTextureKey = textureName;
        SelectedTexture = texture;
    }

    public TextureViewerContext CurrentWindowContext = TextureViewerContext.None;

    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(TextureViewerContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}
