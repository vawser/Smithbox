using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Interface;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

public class TexPropertyView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexPropertyView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Properties##PropertiesView");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.TextureProperties);

        UIHelper.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        UIHelper.WrappedText($"Press {KeyBindings.Current.TEXTURE_ResetZoomLevel.HintText} to reset zoom level to 100%.");

        UIHelper.WrappedText($"");
        UIHelper.WrappedText($"Properties of {Editor.Selection.SelectedTextureKey}:");

        ImGui.BeginChild("TextureProperties");
        Editor.Selection.SwitchWindowContext(TextureViewerContext.TextureProperties);

        if (Editor.Selection.SelectedTexture != null)
        {
            if (Editor.Selection.ViewerTextureResource != null)
            {
                Vector2 size = Editor.DisplayView.GetImageSize(
                    Editor.Selection.ViewerTextureResource, 
                    false);

                Vector2 relativePos = Editor.DisplayView.GetRelativePosition(
                    size, 
                    Editor.Selection.TextureViewWindowPosition, 
                    Editor.Selection.TextureViewScrollPosition);

                ImGui.Columns(2);

                ImGui.Text("Width:");
                ImGui.Text("Height:");
                ImGui.Text("Format:");

                ImGui.NextColumn();

                ImGui.Text($"{size.X}");
                ImGui.Text($"{size.Y}");

                if (Editor.Selection.ViewerTextureResource.GPUTexture != null)
                {
                    ImGui.Text($"{Editor.Selection.ViewerTextureResource.GPUTexture.Format}".ToUpper());
                }
                ImGui.Columns(1);

                ImGui.Text("");
                ImGui.Text($"Relative Position: {relativePos}");

                if (Editor.Project.TextureData.PrimaryBank.ShoeboxEntries != null)
                {
                    var shoeboxEntry = Editor.Project.TextureData.PrimaryBank.ShoeboxEntries.FirstOrDefault();

                    if (shoeboxEntry.Value != null)
                    {
                        if (shoeboxEntry.Value.Textures.ContainsKey(Editor.Selection.SelectedTextureKey))
                        {
                            var subTexs = shoeboxEntry.Value.Textures[Editor.Selection.SelectedTextureKey];
                            foreach (var entry in subTexs)
                            {
                                string IconName;
                                bool IsMatch;
                                (IconName, IsMatch) = Editor.DisplayView.MatchMousePosToIcon(entry, relativePos);

                                if (IsMatch)
                                {
                                    ImGui.Text($"Icon: {IconName}");
                                }
                            }
                        }
                    }
                }
            }
        }

        ImGui.EndChild();

        ImGui.End();
    }
}
