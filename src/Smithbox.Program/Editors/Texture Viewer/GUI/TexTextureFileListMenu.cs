using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexTextureFileListMenu
{
    public TexView Parent;
    public ProjectEntry Project;

    public TexTextureFileListMenu(TexView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        UIHelper.SimpleHeader("Textures", "");

        Parent.Editor.Filters.DisplayTextureFilterSearch();

        ImGui.BeginChild("TextureList", new Vector2(width, height), ImGuiChildFlags.Borders);

        if (Parent.Selection.SelectedTpf != null)
        {
            int index = 0;

            foreach (var entry in Parent.Selection.SelectedTpf.Textures)
            {
                if (Parent.Editor.Filters.IsTextureFilterMatch(entry.Name))
                {
                    var displayName = entry.Name;

                    var isSelected = false;
                    if (Parent.Selection.SelectedTextureKey == entry.Name)
                    {
                        isSelected = true;
                    }

                    // Texture row
                    if (ImGui.Selectable($@"{displayName}", isSelected))
                    {
                        Parent.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                        Parent.Editor.ViewHandler.ActiveView = Parent;
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectTexture)
                    {
                        Parent.Selection.SelectTexture = false;
                        Parent.Selection.SelectTextureEntry(entry.Name, entry);
                        TargetIndex = index;
                        LoadTexture = true;
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectTexture = true;
                        }
                    }
                }

                index++;
            }
        }

        ImGui.EndChild();
    }


    private int TargetIndex = -1;
    public bool LoadTexture = false;

    public void Update()
    {
        if (LoadTexture)
        {
            if (TargetIndex != -1)
            {
                Parent.Selection.ViewerTextureResource = new TextureResource(Parent.Selection.SelectedTpf, TargetIndex);
                Parent.Selection.ViewerTextureResource._LoadTexture(AccessLevel.AccessFull);
            }

            LoadTexture = false;
        }
    }
}
