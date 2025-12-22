using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Utilities;
using System.Diagnostics;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

public class TexToolView
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexToolView(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Display()
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_TextureViewer", ImGuiWindowFlags.MenuBar))
        {
            Editor.Selection.SwitchWindowContext(TextureViewerContext.ToolWindow);

            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            // Export Texture
            if (CFG.Current.Interface_TextureViewer_Tool_ExportTexture)
            {
                if (ImGui.CollapsingHeader("Export Texture"))
                {
                    UIHelper.WrappedText("Export the viewed texture.");
                    UIHelper.WrappedText("");

                    var index = CFG.Current.TextureViewerToolbar_ExportTextureType;

                    UIHelper.WrappedText("Export File Type:");
                    DPI.ApplyInputWidth(windowWidth);
                    if (ImGui.Combo("##ExportType", ref index, Editor.Tools.exportTypes, Editor.Tools.exportTypes.Length))
                    {
                        CFG.Current.TextureViewerToolbar_ExportTextureType = index;
                    }
                    UIHelper.Tooltip("The file type the exported texture will be saved as.");
                    UIHelper.WrappedText("");

                    UIHelper.WrappedText("Export Destination:");
                    DPI.ApplyInputWidth(windowWidth);
                    ImGui.InputText("##exportDestination", ref CFG.Current.TextureViewerToolbar_ExportTextureLocation, 255);
                    if (ImGui.Button("Select", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        string path;
                        var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Destination", out path);
                        if (result)
                        {
                            CFG.Current.TextureViewerToolbar_ExportTextureLocation = path;
                        }
                    }
                    ImGui.SameLine();
                    if (ImGui.Button("View Folder", DPI.HalfWidthButton(windowWidth, 24)))
                    {
                        Process.Start("explorer.exe", CFG.Current.TextureViewerToolbar_ExportTextureLocation);
                    }
                    UIHelper.Tooltip("The folder destination to export the texture to.");
                    UIHelper.WrappedText("");

                    ImGui.Checkbox("Include Container Folder", ref CFG.Current.TextureViewerToolbar_ExportTexture_IncludeFolder);
                    UIHelper.Tooltip("Place the exported texture in a folder with the title of the texture container.");

                    ImGui.Checkbox("Display Export Confirmation", ref CFG.Current.TextureViewerToolbar_ExportTexture_DisplayConfirm);
                    UIHelper.Tooltip("Display the confirmation message box after each export.");
                    UIHelper.WrappedText("");

                    if (ImGui.Button("Export##action_Selection_ExportTexture", DPI.WholeWidthButton(windowWidth, 24)))
                    {
                        Editor.Tools.ExportTextureHandler();
                    }
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Export Texture"))
            {
                CFG.Current.Interface_TextureViewer_Tool_ExportTexture = !CFG.Current.Interface_TextureViewer_Tool_ExportTexture;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_TextureViewer_Tool_ExportTexture);

            ImGui.EndMenu();
        }
    }

    public void DisplayMenubar()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            if (ImGui.MenuItem("Export Texture", KeyBindings.Current.TEXTURE_ExportTexture.HintText))
            {
                Editor.Tools.ExportTextureHandler();
            }
            UIHelper.Tooltip($"Export currently selected texture.");

            ImGui.EndMenu();
        }
    }
}
