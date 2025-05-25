using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Resource;
using StudioCore.Resource.Types;
using StudioCore.TextureViewer;

namespace StudioCore.Editors.TextureViewer.Core;

public class TexTextureView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexTextureView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.Begin("Textures##TextureList");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.TextureList);

        Editor.Filters.DisplayTextureFilterSearch();

        ImGui.BeginChild("TextureList");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.TextureList);

        if (Editor.Selection.SelectedTpf != null)
        {
            int index = 0;

            foreach (var entry in Editor.Selection.SelectedTpf.Textures)
            {
                if (Editor.Filters.IsTextureFilterMatch(entry.Name))
                {
                    var displayName = entry.Name;

                    var isSelected = false;
                    if (Editor.Selection.SelectedTextureKey == entry.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        Editor.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Editor.Selection.SelectTexture)
                    {
                        Editor.Selection.SelectTexture = false;
                        Editor.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        Editor.Selection.SelectTexture = true;
                    }
                }

                index++;
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }


    private int TargetIndex = -1;
    public bool LoadTexture = false;

    public void Update()
    {
        if (!Smithbox.LowRequirementsMode)
        {
            if (LoadTexture)
            {
                if (TargetIndex != -1)
                {
                    Editor.Selection.ViewerTextureResource = new TextureResource(Editor.Selection.SelectedTpf, TargetIndex);
                    Editor.Selection.ViewerTextureResource._LoadTexture(AccessLevel.AccessFull);
                }

                LoadTexture = false;
            }
        }
    }
}
