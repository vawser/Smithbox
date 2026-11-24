using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;
using Veldrid;
using Veldrid.Sdl2;

namespace StudioCore.Editors.Common;

/// <summary>
///     Hosts a top-level editor's window in the editor host: the window, the "loading" placeholder until the editor
///     is made, and forwarding of the frame update and viewport calls. One for every editor, driven by its
///     <see cref="EditorEntry"/>, in place of a stub class per editor.
/// </summary>
public static class EditorStub
{
    /// <summary>
    ///     Draws the editor's window. Called for the editor the host shows, once <see cref="EditorEntry.IsAvailable"/>
    ///     holds.
    /// </summary>
    /// <param name="commands">This frame's editor command, or null. Only passed on (without its endpoint) when it
    /// is for this editor.</param>
    public static unsafe void Display(EditorEntry entry, ProjectEditorHandler handler, float dt, string[] commands)
    {
        if (commands is { Length: > 0 } && commands[0] == entry.Id)
        {
            commands = commands[1..];
            ImGui.SetNextWindowFocus();
        }
        else
        {
            // A command for an editor the project doesn't have: not this editor's to handle
            commands = null;
        }

        // The editors with a viewport draw it behind their window, so the window is see-through where it is
        if (Smithbox.Instance._context.Device == null)
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, *ImGui.GetStyleColorVec4(ImGuiCol.WindowBg));
        }
        else
        {
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.0f, 0.0f, 0.0f, 0.0f));
        }

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0.0f, 0.0f));

        var screen = entry.Screen(handler);

        if (EditorHost.Begin($"{LOC.Get(entry.LabelKey)}###{entry.WindowId}", ImGuiWindowFlags.MenuBar))
        {
            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);

            if (screen != null)
            {
                screen.OnGUI(commands);
            }
            else
            {
                GUI.Spacer();
                ImGui.Text(LOC.Get("EDITOR_Editor_Is_Loading"));
            }

            ImGui.End();

            if (screen != null)
            {
                handler.FocusedEditor = screen;

                // Time-dependent viewport work: a no-op for the editors without a viewport
                screen.Update(dt);
            }
        }
        else
        {
            if (entry.DefocusWhenHidden)
            {
                screen?.OnDefocus();
            }

            ImGui.PopStyleColor(1);
            ImGui.PopStyleVar(1);
            ImGui.End();
        }
    }

    /// <summary>
    ///     Renders the focused editor's viewport, if it has one. Only the focused editor is shown, so it is the only
    ///     one drawn.
    /// </summary>
    public static void Draw(ProjectEditorHandler handler, GraphicsDevice device, CommandList cl)
    {
        handler.FocusedEditor?.Draw(device, cl);
    }

    /// <summary>
    ///     Tells the focused editor the window was resized, for its viewport's render targets
    /// </summary>
    public static void EditorResized(ProjectEditorHandler handler, Sdl2Window window, GraphicsDevice device)
    {
        handler.FocusedEditor?.EditorResized(window, device);
    }
}
