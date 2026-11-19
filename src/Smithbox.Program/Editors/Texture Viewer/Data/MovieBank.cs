using SoulsFormats;
using StudioCore.Application;
using Andre.IO.VFS;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Scaleform movies (/menu/*.gfx) of the Texture Viewer's GFx mode: the file list, the movies read so far, and
/// their saves to the project (loose, as the game reads them). See Claude/Documentation/Scaleform Editor Analysis.md.
/// </summary>
public class MovieBank
{
    public ProjectEntry Project;
    public VirtualFileSystem TargetFS;

    /// <summary>Every movie of the project's file list; the value is null until the movie is read.</summary>
    public Dictionary<FileDictionaryEntry, MovieContents> Entries = new();

    /// <summary>Whether a movie comes from the project rather than the game data, by path.</summary>
    public Dictionary<string, bool> ProjectFiles = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>The images of the movies, resolved to shoebox cells and TPF textures.</summary>
    public MovieImages Images;

    public MovieBank(ProjectEntry project, VirtualFileSystem targetFs)
    {
        Project = project;
        TargetFS = targetFs;
        Images = new MovieImages(project);
        Configuration = new MovieConfiguration(project);
        Images.Configuration = Configuration;
    }

    /// <summary>The project's Scaleform Configuration (placeholder substitutes for the preview).</summary>
    public MovieConfiguration Configuration;

    public void Setup()
    {
        var entries = new Dictionary<FileDictionaryEntry, MovieContents>();
        var projectFiles = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in Project.Locator.MovieFiles?.Entries ?? Enumerable.Empty<FileDictionaryEntry>())
        {
            entries[entry] = null;

            try
            {
                projectFiles[entry.Path] = Project.VFS.ProjectFS.FileExists(entry.Path);
            }
            catch
            {
                projectFiles[entry.Path] = false;
            }
        }

        Entries = entries;
        ProjectFiles = projectFiles;

        Configuration.Load();
    }

    public bool IsProjectFile(FileDictionaryEntry entry)
    {
        return entry != null && ProjectFiles.TryGetValue(entry.Path, out var isProject) && isProject;
    }

    /// <summary>The movie, read on first use. Null when it can't be read (logged).</summary>
    public MovieContents Load(FileDictionaryEntry entry)
    {
        if (entry == null)
            return null;

        if (Entries.TryGetValue(entry, out var loaded) && loaded != null)
            return loaded;

        try
        {
            var data = TargetFS.ReadFile(entry.Path);
            if (data == null)
            {
                Smithbox.LogError(this, LOC.Get("TEXVIEW_Movie_Log_Not_Found", entry.Path));
                return null;
            }

            var movie = GFX.Read(data.Value);

            if (movie.TypedFallbacks.Count > 0)
            {
                Smithbox.Log(this, LOC.Get("TEXVIEW_Movie_Log_Raw_Tags", entry.Path, movie.TypedFallbacks.Count));
            }

            var contents = new MovieContents(entry, movie);
            Entries[entry] = contents;
            return contents;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_Movie_Log_Read_Failed", entry.Path), e);
            return null;
        }
    }

    public IEnumerable<MovieContents> ModifiedMovies()
    {
        return Entries.Values.Where(m => m != null && m.IsModified);
    }

    public bool HasUnsavedChanges => ModifiedMovies().Any();

    public List<string> GetUnsavedFiles()
    {
        return ModifiedMovies().Select(m => m.Entry.Path).OrderBy(p => p).ToList();
    }

    /// <summary>Writes every edited movie to the project. True if all were written.</summary>
    public bool SaveAll()
    {
        var success = true;

        foreach (var movie in ModifiedMovies().ToList())
        {
            try
            {
                Project.VFS.ProjectFS.WriteFile(movie.Entry.Path, movie.Movie.Write());
                movie.IsModified = false;
                ProjectFiles[movie.Entry.Path] = true;
                Smithbox.Log(this, LOC.Get("TEXVIEW_Movie_Log_Saved", movie.Entry.Path));
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TEXVIEW_Movie_Log_Save_Failed", movie.Entry.Path), e);
                success = false;
            }
        }

        return success;
    }

    public void Dispose()
    {
        Images?.Dispose();
        Entries = null;
    }
}

/// <summary>
/// A movie read for editing, with its character table.
/// </summary>
public class MovieContents
{
    public FileDictionaryEntry Entry { get; }
    public GFX Movie { get; }

    /// <summary>Characters by id (ids do not change while editing).</summary>
    public Dictionary<int, GFX.Tag> Characters { get; }

    /// <summary>How many placements use each sprite, so an edit inside a shared symbol can say it moves every use.</summary>
    public Dictionary<int, int> SpriteUses { get; } = new();

    /// <summary>An edit was made (or undone) since the last save.</summary>
    public bool IsModified { get; set; }

    /// <summary>Bumped by every edit, so views know to evaluate the display list again.</summary>
    public int Version { get; private set; }

    public MovieContents(FileDictionaryEntry entry, GFX movie)
    {
        Entry = entry;
        Movie = movie;
        Characters = movie.GetCharacters();

        foreach (var place in movie.AllTags().OfType<GFX.PlaceObject>())
        {
            if (place.CharacterId is ushort id && Characters.TryGetValue(id, out var c) && c is GFX.DefineSprite)
                SpriteUses[id] = SpriteUses.GetValueOrDefault(id) + 1;
        }
    }

    public void MarkChanged()
    {
        IsModified = true;
        Version++;
    }

    /// <summary>A change that is not an edit (a live drag before it is committed): views evaluate again.</summary>
    public void MarkDirty()
    {
        Version++;
    }
}
