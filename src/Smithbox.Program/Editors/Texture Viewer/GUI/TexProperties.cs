using Hexa.NET.DirectXTex;
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
        ImGui.BeginChild("TextureProperties", new Vector2(0, 0), ImGuiChildFlags.Borders);

        UIHelper.WrappedText($"Hold Left-Control and scroll the mouse wheel to zoom in and out.");
        UIHelper.WrappedText($"Press {InputManager.GetHint(KeybindID.TextureViewer_Reset_Zoom_Level)} to reset zoom level to 100%.");
        UIHelper.WrappedText("");

        UIHelper.SimpleHeader($"Properties of {Parent.Selection.SelectedTextureKey}", "");

        float sizeX = -1;
        float sizeY = -1;
        Vector2 relativePos = new Vector2();
        string format = "";
        string iconName = "";

        if (Parent.Selection.SelectedTexture != null)
        {
            if (Parent.Selection.ViewerTextureResource != null)
            {
                Vector2 size = Parent.DisplayViewport.GetImageSize(
                    Parent.Selection.ViewerTextureResource, 
                    false);

                sizeX = size.X;
                sizeY = size.Y;

                relativePos = Parent.DisplayViewport.GetRelativePosition(
                    size,
                    Parent.Selection.TextureViewWindowPosition,
                    Parent.Selection.TextureViewScrollPosition);

                if (Parent.Selection.ViewerTextureResource.GPUTexture != null)
                {
                    format = $"{Parent.Selection.ViewerTextureResource.GPUTexture.Format}".ToUpper();
                }

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
                                (IconName, IsMatch) = Parent.DisplayViewport.MatchMousePosToIcon(entry, relativePos);

                                if (IsMatch)
                                {
                                    iconName = IconName;
                                }
                            }
                        }
                    }
                }
            }
        }

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        // Display
        if (ImGui.BeginTable($"propertiesTable", 2, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            var colFlags = ImGuiTableColumnFlags.WidthStretch;

            ImGui.TableSetupColumn("Property", colFlags, 0.4f);
            ImGui.TableSetupColumn("Value", colFlags);
            ImGui.TableSetupScrollFreeze(2, 1);

            // ID
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("ID");

            // Name
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Name");

            // Size Y
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"Size: Y");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{sizeY}");

            // Size X
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"Size: X");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{sizeX}");

            // Relative Position: X
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"Relative Position: X");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{relativePos.X}");

            // Relative Position: Y
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"Relative Position: Y");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{relativePos.Y}");

            // Format
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text($"Format");

            ImGui.TableSetColumnIndex(1);
            ImGui.Text($"{format}");

            // Icon
            if (iconName != "")
            {
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);
                ImGui.Text($"Icon");

                ImGui.TableSetColumnIndex(1);
                ImGui.Text($"{iconName}");
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
}
