using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexProperties
{
    public TexEditorView Parent;
    public ProjectEntry Project;

    public TexProperties(TexEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the file container list view
    /// </summary>
    public void Display()
    {
        UIHelper.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        UIHelper.WrappedText($"Press {InputManager.GetHint(KeybindID.TextureViewer_Reset_Zoom_Level)} to reset zoom level to 100%.");

        UIHelper.WrappedText($"");
        UIHelper.WrappedText($"Properties of {Parent.Selection.SelectedTextureKey}:");

        ImGui.BeginChild("TextureProperties");

        if (Parent.Selection.SelectedTexture != null)
        {
            if (Parent.Selection.ViewerTextureResource != null)
            {
                Vector2 size = Parent.Display.GetImageSize(
                    Parent.Selection.ViewerTextureResource, 
                    false);

                Vector2 relativePos = Parent.Display.GetRelativePosition(
                    size,
                    Parent.Selection.TextureViewWindowPosition,
                    Parent.Selection.TextureViewScrollPosition);

                ImGui.Columns(2);

                ImGui.Text("Width:");
                ImGui.Text("Height:");
                ImGui.Text("Format:");

                ImGui.NextColumn();

                ImGui.Text($"{size.X}");
                ImGui.Text($"{size.Y}");

                if (Parent.Selection.ViewerTextureResource.GPUTexture != null)
                {
                    ImGui.Text($"{Parent.Selection.ViewerTextureResource.GPUTexture.Format}".ToUpper());
                }
                ImGui.Columns(1);

                ImGui.Text("");
                ImGui.Text($"Relative Position: {relativePos}");

                if (Project.Handler.TextureData.PrimaryBank.ShoeboxEntries != null)
                {
                    var shoeboxEntry = Project.Handler.TextureData.PrimaryBank.ShoeboxEntries.FirstOrDefault();

                    if (shoeboxEntry.Value != null)
                    {
                        if (shoeboxEntry.Value.Textures.ContainsKey(Parent.Selection.SelectedTextureKey))
                        {
                            var subTexs = shoeboxEntry.Value.Textures[Parent.Selection.SelectedTextureKey];
                            foreach (var entry in subTexs)
                            {
                                string IconName;
                                bool IsMatch;
                                (IconName, IsMatch) = Parent.Display.MatchMousePosToIcon(entry, relativePos);

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
    }
}
