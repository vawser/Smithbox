using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using Veldrid;

namespace StudioCore.Editors.TextureViewer;

public class TexShortcuts
{
    public TextureViewerScreen Editor;
    public ProjectEntry Project;

    public TexShortcuts(TextureViewerScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Monitor()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (InputManager.IsPressed(KeybindID.Toggle_Tools_Menu))
        {
            CFG.Current.Interface_TextureViewer_ToolWindow = !CFG.Current.Interface_TextureViewer_ToolWindow;
        }

        if (activeView == null)
            return;

        if (FocusManager.IsInTextureViewer())
        {
            // Save
            if (InputManager.IsPressed(KeybindID.Save))
            {
                Editor.Save();
            }

            // Undo / Redo
            if (activeView.ActionManager.CanUndo() && InputManager.IsPressed(KeybindID.Undo))
            {
                activeView.ActionManager.UndoAction();
            }

            if (activeView.ActionManager.CanRedo() && InputManager.IsPressed(KeybindID.Redo))
            {
                activeView.ActionManager.RedoAction();
            }

            // List commands act on the list under the mouse, never while a text field has the keyboard
            if (!Hexa.NET.ImGui.ImGui.GetIO().WantTextInput)
            {
                ListShortcuts(activeView);
            }
        }

        if (InputManager.IsPressed(KeybindID.TextureViewer_Export_Texture))
        {
            activeView.ToolView.TextureExport.ExportTextureHandler();
        }

        if (InputManager.HasCtrlDown())
        {
            activeView.ZoomState.HandleZoom();
        }

        if (InputManager.IsPressed(KeybindID.TextureViewer_Reset_Zoom_Level))
        {
            activeView.ZoomState.ZoomReset();
        }
    }

    /// <summary>Copy / Paste / Duplicate / Delete for the texture list and the TPF list (T3).</summary>
    private void ListShortcuts(TexEditorView view)
    {
        var sel = view.Selection;

        if (view.FocusedList == TexEditorView.ListFocus.Textures)
        {
            var target = view.GetEditTarget();

            if (InputManager.IsPressed(KeybindID.Copy))
                view.Commands.CopyTexture(sel.SelectedTpf, sel.SelectedTexture);

            if (InputManager.IsPressed(KeybindID.Paste))
                view.Commands.PasteTextures();

            if (target != null && sel.SelectedTexture != null)
            {
                if (InputManager.IsPressed(KeybindID.Duplicate))
                    view.TextureList.Duplicate(target, sel.SelectedTexture);

                if (InputManager.IsPressed(KeybindID.Delete))
                    view.TextureList.Delete(target, sel.SelectedTexture);
            }
        }
        else if (view.FocusedList == TexEditorView.ListFocus.Tpfs && sel.SelectedTpfBinderFile != null)
        {
            if (InputManager.IsPressed(KeybindID.Paste))
                view.Commands.PasteTextures();

            if (InputManager.IsPressed(KeybindID.Duplicate))
                view.Commands.DuplicateTpf(sel.SelectedTpfBinderFile);

            if (InputManager.IsPressed(KeybindID.Delete))
                view.Commands.DeleteTpf(sel.SelectedTpfBinderFile);
        }
    }
}
