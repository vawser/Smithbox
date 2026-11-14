using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// One side of a <see cref="ModelEdgeEditAction"/>: an EDGE file object, its ID, and the entry objects it held with
/// their values. <see cref="Apply"/> puts that file back in place (the same file object, the same entry objects, those
/// values), so anything holding the file or an entry (the EDGE node, the property grid and its own undo steps) still
/// finds it. Uses only SoulsFormats, so EdgeSurvey's object check compiles this same file.
/// </summary>
public sealed class ModelEdgeFileState
{
    public EDGE File { get; }
    public int Id { get; }

    private readonly List<EDGE.Edge> Entries;
    private readonly List<EDGE.Edge> Values;

    private ModelEdgeFileState(EDGE file, int id, List<EDGE.Edge> entries)
    {
        File = file;
        Id = id;
        Entries = new List<EDGE.Edge>(entries);
        Values = new List<EDGE.Edge>(entries.Count);

        foreach (var entry in entries)
        {
            Values.Add(entry.Clone());
        }
    }

    /// <summary>The file as it is now, with the entries it holds.</summary>
    public static ModelEdgeFileState Capture(EDGE file) => new(file, file.ID, file.Edges);

    /// <summary>
    /// The contents of <paramref name="contents"/> to be put into <paramref name="file"/>, with new entry objects (the
    /// contents' own aren't kept).
    /// </summary>
    public static ModelEdgeFileState Into(EDGE file, EDGE contents)
    {
        var entries = new List<EDGE.Edge>(contents.Edges.Count);

        foreach (var entry in contents.Edges)
        {
            entries.Add(entry.Clone());
        }

        return new ModelEdgeFileState(file, contents.ID, entries);
    }

    /// <summary>Puts this side's ID, entries and values into its file.</summary>
    public void Apply()
    {
        File.ID = Id;
        File.Edges.Clear();

        for (var i = 0; i < Entries.Count; i++)
        {
            CopyValues(Values[i], Entries[i]);
            File.Edges.Add(Entries[i]);
        }
    }

    /// <summary>Copies every field of an entry onto another.</summary>
    public static void CopyValues(EDGE.Edge from, EDGE.Edge to)
    {
        to.Type = from.Type;
        to.V1 = from.V1;
        to.V2 = from.V2;
        to.V3 = from.V3;
        to.Unk2C = from.Unk2C;
        to.Unk30 = from.Unk30;
        to.VariationID = from.VariationID;
        to.Unk36 = from.Unk36;
    }

    /// <summary>
    /// Writes a copy's values into a file's own entries, when they have as many (a move never adds or removes one):
    /// the live preview of a drag, which the action at its end records. False if the counts differ.
    /// </summary>
    public static bool PreviewInto(EDGE file, EDGE values)
    {
        if (values == null || values.Edges.Count != file.Edges.Count)
            return false;

        for (var i = 0; i < file.Edges.Count; i++)
        {
            CopyValues(values.Edges[i], file.Edges[i]);
        }

        return true;
    }
}
