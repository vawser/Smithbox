using Hexa.NET.ImGui;
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

public class TexTexturePropertyView
{
    private TextureViewerScreen Screen;
    private TexViewSelection Selection;
    private ShoeboxLayoutContainer ShoeboxLayouts;
    private TexTextureViewport TextureViewport;

    public TexTexturePropertyView(TextureViewerScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ShoeboxLayouts = screen.ShoeboxLayouts;
        TextureViewport = screen.TextureViewport;
    }

    // <summary>
    /// Reset view state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        ImGui.Begin("Properties##PropertiesView");
        Selection.SwitchWindowContext(TextureViewerContext.TextureProperties);

        UIHelper.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        UIHelper.WrappedText($"Press {KeyBindings.Current.TEXTURE_ResetZoomLevel.HintText} to reset zoom level to 100%.");

        UIHelper.WrappedText($"");
        UIHelper.WrappedText($"Properties of {Selection.CurrentTextureName}:");

        ImGui.BeginChild("TextureProperties");
        Selection.SwitchWindowContext(TextureViewerContext.TextureProperties);

        if (Selection._selectedTexture != null)
        {
            if (Selection.CurrentTextureInView != null)
            {
                Vector2 size = TextureViewport.GetImageSize(Selection.CurrentTextureInView, false);
                Vector2 relativePos = TextureViewport.GetRelativePosition(size, Selection.TextureViewWindowPosition, Selection.TextureViewScrollPosition);

                ImGui.Columns(2);

                ImGui.Text("Width:");
                ImGui.Text("Height:");
                ImGui.Text("Format:");

                ImGui.NextColumn();

                ImGui.Text($"{size.X}");
                ImGui.Text($"{size.Y}");

                if (Selection.CurrentTextureInView.GPUTexture != null)
                {
                    ImGui.Text($"{Selection.CurrentTextureInView.GPUTexture.Format}".ToUpper());
                }
                ImGui.Columns(1);

                ImGui.Text("");
                ImGui.Text($"Relative Position: {relativePos}");

                if (ShoeboxLayouts != null)
                {
                    if (ShoeboxLayouts.Textures.ContainsKey(Selection.CurrentTextureName))
                    {
                        var subTexs = ShoeboxLayouts.Textures[Selection.CurrentTextureName];
                        foreach (var entry in subTexs)
                        {
                            string IconName;
                            bool IsMatch;
                            (IconName, IsMatch) = TextureViewport.MatchMousePosToIcon(entry, relativePos);

                            if (IsMatch)
                            {
                                ImGui.Text($"Icon: {IconName}");
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
