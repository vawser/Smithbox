using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Scaleform Configuration tool (Texture Viewer Tools window): assigns an image to show in the GFx mode's preview
/// in place of a placeholder image (one the game fills in at runtime, or one no atlas has). Preview only: the movies
/// are not changed. Entries are the project's, written at once to .smithbox/Project/Scaleform Configuration.json.
/// </summary>
public class ScaleformConfigurationTool
{
    private readonly TexEditorView View;
    private readonly ProjectEntry Project;

    private string PlaceholderBuffer = "";
    private string SubstituteBuffer = "";

    private List<string> KnownNames = new();
    private DateTime KnownNamesAt = DateTime.MinValue;

    public ScaleformConfigurationTool(TexEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private MovieBank Movies => Project.Handler?.TextureData?.PrimaryBank?.Movies;

    /// <summary>The project has menu movies, so the tool (and the Tools window's GFX category) is shown.</summary>
    public bool IsAvailable => Movies?.Entries != null && Movies.Entries.Count > 0;

    public void OnToolWindow()
    {
        var movies = Movies;
        if (!IsAvailable)
            return;

        if (!ToolSections.Header($"{LOC.Get("TEXVIEW_ScaleformConfig_Header")}##scaleformConfigHeader"))
            return;

        ImGui.BeginChild("ScaleformConfigToolSection", ImGuiChildFlags.Borders | ImGuiChildFlags.AutoResizeY);

        Display(movies);

        ImGui.EndChild();
    }

    private void Display(MovieBank movies)
    {
        var config = movies.Configuration;

        GUI.WrappedText(LOC.Get("TEXVIEW_ScaleformConfig_Hint"));
        GUI.Spacer();

        // Add or change: several fields, so only the Set button applies them
        GUI.WrappedText(LOC.Get("TEXVIEW_ScaleformConfig_Placeholder"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##scaleformPlaceholder", ref PlaceholderBuffer, 255);
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformConfig_Placeholder_TT"));

        GUI.WrappedText(LOC.Get("TEXVIEW_ScaleformConfig_Substitute"));
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##scaleformSubstitute", ref SubstituteBuffer, 255);
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformConfig_Substitute_TT"));

        Suggestions(movies);

        if (SubstituteBuffer.Trim().Length > 0)
        {
            var found = movies.Images.Lookup(SubstituteBuffer.Trim(), out var pending);
            if (found != null)
                ImGui.TextDisabled(LOC.Get("TEXVIEW_ScaleformConfig_Substitute_Found", found.Source));
            else if (pending)
                ImGui.TextDisabled(LOC.Get("TEXVIEW_Movie_Image_Loading"));
            else
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("TEXVIEW_ScaleformConfig_Substitute_Not_Found"));
        }

        var canSet = PlaceholderBuffer.Trim().Length > 0 && SubstituteBuffer.Trim().Length > 0;
        ImGui.BeginDisabled(!canSet);
        if (ImGui.Button($"{LOC.Get("TEXVIEW_ScaleformConfig_Set")}##scaleformSet"))
        {
            config.Set(PlaceholderBuffer, SubstituteBuffer);
            PlaceholderBuffer = "";
            SubstituteBuffer = "";
        }
        ImGui.EndDisabled();
        GUI.Tooltip(LOC.Get("TEXVIEW_ScaleformConfig_Set_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("TEXVIEW_ScaleformConfig_Clear")}##scaleformClear"))
        {
            PlaceholderBuffer = "";
            SubstituteBuffer = "";
        }

        GUI.Spacer();
        OpenMovieImages(movies);

        GUI.Spacer();
        Entries(movies);

        GUI.Spacer();
        ImGui.TextDisabled(config.FilePath);
    }

    /// <summary>Names the preview can show that contain the typed text, to pick one.</summary>
    private void Suggestions(MovieBank movies)
    {
        var text = SubstituteBuffer.Trim();
        if (text.Length < 2)
            return;

        if ((DateTime.Now - KnownNamesAt).TotalSeconds > 2)
        {
            KnownNames = movies.Images.KnownNames();
            KnownNamesAt = DateTime.Now;
        }

        var matches = KnownNames.Where(n => n.Contains(text, StringComparison.OrdinalIgnoreCase)).Take(40).ToList();
        if (matches.Count == 0 || (matches.Count == 1 && matches[0].Equals(text, StringComparison.OrdinalIgnoreCase)))
            return;

        var height = Math.Min(matches.Count, 8) * ImGui.GetTextLineHeightWithSpacing() + 6;
        ImGui.BeginChild("scaleformSuggestions", new Vector2(0, height), ImGuiChildFlags.Borders);
        foreach (var name in matches)
        {
            if (ImGui.Selectable($"{name}##suggest{name}"))
                SubstituteBuffer = name;

            if (ImGui.IsItemHovered())
                PreviewTooltip(movies, name);
        }
        ImGui.EndChild();
    }

    /// <summary>The open movie's images that are not found (or are shown as a substitute), to pick as placeholders.</summary>
    private void OpenMovieImages(MovieBank movies)
    {
        var movie = View.Movie;
        if (!movie.IsActive)
        {
            GUI.WrappedText(LOC.Get("TEXVIEW_ScaleformConfig_No_Movie"));
            return;
        }

        var images = movie.Contents.Characters.Values.OfType<GFX.DefineExternalImage2>()
            .Select(i => i.FileName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(name => (Name: name, Shown: movies.Images.Get(name, out var pending), Pending: pending))
            .Where(i => !i.Pending && (i.Shown == null || i.Shown.IsSubstitute))
            .OrderBy(i => i.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (!ImGui.CollapsingHeader($"{LOC.Get("TEXVIEW_ScaleformConfig_Open_Movie", movie.Entry.Filename, images.Count)}###scaleformOpenMovie"))
            return;

        if (images.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("TEXVIEW_ScaleformConfig_Open_Movie_None"));
            return;
        }

        foreach (var (name, shown, _) in images)
        {
            var stem = StemOf(name);
            if (ImGui.Selectable($"{stem}{(shown != null ? "  →" : "")}##placeholder{name}"))
                PlaceholderBuffer = stem;
            GUI.Tooltip(shown != null ? shown.Source : LOC.Get("TEXVIEW_ScaleformConfig_Pick_Placeholder_TT"));
        }
    }

    private void Entries(MovieBank movies)
    {
        var config = movies.Configuration;

        if (!ImGui.CollapsingHeader($"{LOC.Get("TEXVIEW_ScaleformConfig_Entries", config.Placeholders.Count)}###scaleformEntries", ImGuiTreeNodeFlags.DefaultOpen))
            return;

        if (config.Placeholders.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("TEXVIEW_ScaleformConfig_Entries_None"));
            return;
        }

        if (!ImGui.BeginTable("scaleformEntriesTable", 3, ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingStretchProp))
            return;

        ImGui.TableSetupColumn(LOC.Get("TEXVIEW_ScaleformConfig_Placeholder"));
        ImGui.TableSetupColumn(LOC.Get("TEXVIEW_ScaleformConfig_Substitute"));
        ImGui.TableSetupColumn("##actions", ImGuiTableColumnFlags.WidthFixed);
        ImGui.TableHeadersRow();

        MovieConfiguration.Entry remove = null;
        foreach (var entry in config.Placeholders)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text(entry.Placeholder);

            ImGui.TableSetColumnIndex(1);
            var found = movies.Images.Lookup(entry.Substitute, out var pending);
            if (found == null && !pending)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, entry.Substitute);
            else
                ImGui.Text(entry.Substitute);
            if (ImGui.IsItemHovered())
                PreviewTooltip(movies, entry.Substitute);

            ImGui.TableSetColumnIndex(2);
            if (ImGui.SmallButton($"{LOC.Get("TEXVIEW_ScaleformConfig_Edit")}##edit{entry.Placeholder}"))
            {
                PlaceholderBuffer = entry.Placeholder;
                SubstituteBuffer = entry.Substitute;
            }
            ImGui.SameLine();
            if (ImGui.SmallButton($"{LOC.Get("TEXVIEW_ScaleformConfig_Remove")}##remove{entry.Placeholder}"))
                remove = entry;
        }

        ImGui.EndTable();

        if (remove != null)
            config.Remove(remove);
    }

    private static void PreviewTooltip(MovieBank movies, string name)
    {
        var image = movies.Images.Lookup(name, out var pending);
        ImGui.BeginTooltip();
        ImGui.Text(name);
        ImGui.TextDisabled(image?.Source ?? (pending ? LOC.Get("TEXVIEW_Movie_Image_Loading") : LOC.Get("TEXVIEW_Movie_Image_Missing")));
        if (image != null && image.IsReady)
        {
            var max = 128 * DPI.UIScale();
            var size = image.Size.X > 0 && image.Size.Y > 0 ? image.Size * (max / Math.Max(image.Size.X, image.Size.Y)) : new Vector2(max);
            ImGui.Image(image.TextureId, size, image.Uv0, image.Uv1);
        }
        ImGui.EndTooltip();
    }

    /// <summary>The name without image extensions, keeping its case (MENU_Icon_00000.tga → MENU_Icon_00000).</summary>
    private static string StemOf(string name)
    {
        var stem = MovieImages.Stem(name);
        return name.Length >= stem.Length ? name[..stem.Length] : stem;
    }
}
