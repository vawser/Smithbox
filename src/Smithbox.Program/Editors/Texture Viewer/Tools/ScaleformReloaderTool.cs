using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Scaleform Reloader tool (Texture Viewer Tools window, GFX): reloads saved menu movies in the running game.
/// Shown when the project's game has Scaleform Reloader offsets. See ScaleformReloader.
/// </summary>
public class ScaleformReloaderTool
{
    private readonly TexEditorView View;
    private readonly ProjectEntry Project;

    private bool HooksEnabled = true;

    public ScaleformReloaderTool(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MovieBank Movies => Project.Handler?.TextureData?.PrimaryBank?.Movies;

    public bool IsAvailable => Movies?.Reloader != null;

    public void OnToolWindow()
    {
        var reloader = Movies?.Reloader;
        if (reloader == null)
            return;

        if (!ToolSections.Header($"{LOC.Get("TEXVIEW_ScaleformReloader_Header")}##scaleformReloaderHeader"))
            return;

        ImGui.BeginChild("ScaleformReloaderToolSection", ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

        Display(reloader);

        ImGui.EndChild();
    }

    private void Display(ScaleformReloader reloader)
    {
        reloader.Drain();

        GUI.WrappedText(LOC.Get("TEXVIEW_ScaleformReloader_Hint"));
        ImGui.TextDisabled(LOC.Get("TEXVIEW_ScaleformReloader_Supported",
            string.Join(", ", reloader.Offsets.Select(o => $"{o.Game} ({o.ExeName} {o.ExeVersion})"))));
        GUI.Spacer();

        if (reloader.IsAttached)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, reloader.Status);
        else
            ImGui.TextDisabled(LOC.Get("TEXVIEW_ScaleformReloader_Status_Not_Attached"));

        ImGui.Checkbox(LOC.Get("TEXVIEW_ScaleformReloader_Reload_On_Save"), ref reloader.ReloadOnSave);
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformReloader_Reload_On_Save_TT"));

        var busy = reloader.IsBusy;
        int saved;
        string[] savedPaths;
        lock (reloader.SavedSinceReload)
        {
            saved = reloader.SavedSinceReload.Count;
            savedPaths = reloader.SavedSinceReload.ToArray();
        }

        ImGui.BeginDisabled(busy || saved == 0);
        if (ImGui.Button($"{LOC.Get("TEXVIEW_ScaleformReloader_Reload_Saved", saved)}##scaleformReloadSaved"))
            reloader.Reload(savedPaths);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformReloader_Reload_Saved_TT"));

        var movie = View.Movie;
        var canOpen = movie.IsActive && Movies.IsProjectFile(movie.Entry) && !movie.Contents.IsModified;
        ImGui.BeginDisabled(busy || !canOpen);
        if (ImGui.Button($"{LOC.Get("TEXVIEW_ScaleformReloader_Reload_Open")}##scaleformReloadOpen"))
            reloader.Reload([movie.Entry.Path]);
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get(!movie.IsActive ? "TEXVIEW_ScaleformReloader_Reload_Open_None_TT"
            : movie.Contents.IsModified ? "TEXVIEW_ScaleformReloader_Reload_Open_Unsaved_TT"
            : !Movies.IsProjectFile(movie.Entry) ? "TEXVIEW_ScaleformReloader_Reload_Open_Not_Project_TT"
            : "TEXVIEW_ScaleformReloader_Reload_Open_TT"));

        ImGui.BeginDisabled(busy);
        if (ImGui.Button($"{LOC.Get("TEXVIEW_ScaleformReloader_List")}##scaleformList"))
            reloader.ListOnScreen();
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformReloader_List_TT"));

        if (ImGui.Checkbox(LOC.Get("TEXVIEW_ScaleformReloader_Hooks_Enabled"), ref HooksEnabled))
            reloader.SetEnabled(HooksEnabled);
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformReloader_Hooks_Enabled_TT"));
        ImGui.EndDisabled();

        if (busy)
            ImGui.TextDisabled(LOC.Get("TEXVIEW_ScaleformReloader_Working"));

        if (reloader.History.Count > 0)
        {
            GUI.Spacer();
            var height = System.Math.Min(reloader.History.Count, 10) * ImGui.GetTextLineHeightWithSpacing() + 8;
            ImGui.BeginChild("scaleformReloaderLog", new Vector2(0, height), ImGuiChildFlags.Borders);
            foreach (var (time, error, text) in reloader.History.AsEnumerable().Reverse().Take(30))
            {
                if (error)
                    ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, $"{time:HH:mm:ss}  {text}");
                else
                    ImGui.TextWrapped($"{time:HH:mm:ss}  {text}");
            }
            ImGui.EndChild();
        }
    }
}
