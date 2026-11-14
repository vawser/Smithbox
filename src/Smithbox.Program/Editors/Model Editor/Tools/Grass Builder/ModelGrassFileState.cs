using SoulsFormats;
using System.Collections.Generic;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// One side of a <see cref="ModelGrassEditAction"/>: a GRASS file object and the volume, vertex and face objects it
/// held, with their values. <see cref="Apply"/> puts that file back in place (the same file object and lists, the same
/// entry objects, those values), so anything holding the file or an entry (the GRASS node, the property grid and its
/// own undo steps) still finds it. Uses only SoulsFormats, so GrassSurvey's build check compiles this same file.
/// </summary>
public sealed class ModelGrassFileState
{
    public GRASS File { get; }

    private readonly List<GRASS.Volume> Volumes;
    private readonly List<GRASS.Volume> VolumeValues;
    private readonly List<GRASS.Vertex> Vertices;
    private readonly List<GRASS.Vertex> VertexValues;
    private readonly List<GRASS.Face> Faces;
    private readonly List<GRASS.Face> FaceValues;

    private ModelGrassFileState(GRASS file, List<GRASS.Volume> volumes, List<GRASS.Vertex> vertices, List<GRASS.Face> faces)
    {
        File = file;

        Volumes = new List<GRASS.Volume>(volumes);
        Vertices = new List<GRASS.Vertex>(vertices);
        Faces = new List<GRASS.Face>(faces);

        VolumeValues = volumes.ConvertAll(Clone);
        VertexValues = vertices.ConvertAll(Clone);
        FaceValues = faces.ConvertAll(Clone);
    }

    /// <summary>The file as it is now, with the entries it holds.</summary>
    public static ModelGrassFileState Capture(GRASS file) =>
        new(file, file.BoundingVolumeHierarchy, file.Vertices, file.Faces);

    /// <summary>
    /// The contents of <paramref name="contents"/> to be put into <paramref name="file"/>, with new entry objects (the
    /// contents' own aren't kept).
    /// </summary>
    public static ModelGrassFileState Into(GRASS file, GRASS contents) =>
        new(file,
            contents.BoundingVolumeHierarchy.ConvertAll(Clone),
            contents.Vertices.ConvertAll(Clone),
            contents.Faces.ConvertAll(Clone));

    public int VertexCount => Vertices.Count;
    public int FaceCount => Faces.Count;

    /// <summary>Puts this side's entries and values into its file.</summary>
    public void Apply()
    {
        File.BoundingVolumeHierarchy.Clear();
        File.Vertices.Clear();
        File.Faces.Clear();

        for (var i = 0; i < Volumes.Count; i++)
        {
            CopyValues(VolumeValues[i], Volumes[i]);
            File.BoundingVolumeHierarchy.Add(Volumes[i]);
        }

        for (var i = 0; i < Vertices.Count; i++)
        {
            CopyValues(VertexValues[i], Vertices[i]);
            File.Vertices.Add(Vertices[i]);
        }

        for (var i = 0; i < Faces.Count; i++)
        {
            CopyValues(FaceValues[i], Faces[i]);
            File.Faces.Add(Faces[i]);
        }
    }

    private static GRASS.Volume Clone(GRASS.Volume v)
    {
        var copy = new GRASS.Volume();
        CopyValues(v, copy);
        return copy;
    }

    private static GRASS.Vertex Clone(GRASS.Vertex v)
    {
        var copy = new GRASS.Vertex();
        CopyValues(v, copy);
        return copy;
    }

    private static GRASS.Face Clone(GRASS.Face f)
    {
        var copy = new GRASS.Face();
        CopyValues(f, copy);
        return copy;
    }

    private static void CopyValues(GRASS.Volume from, GRASS.Volume to)
    {
        to.StartChildIndex = from.StartChildIndex;
        to.EndChildIndex = from.EndChildIndex;
        to.StartFaceIndex = from.StartFaceIndex;
        to.EndFaceIndex = from.EndFaceIndex;
        to.Unk10 = from.Unk10;
        to.BoundingBox = from.BoundingBox;
    }

    private static void CopyValues(GRASS.Vertex from, GRASS.Vertex to)
    {
        to.Position = from.Position;

        // The densities array is fixed (private setter), so its values are copied into it
        for (var s = 0; s < from.GrassDensities.Length && s < to.GrassDensities.Length; s++)
        {
            to.GrassDensities[s] = from.GrassDensities[s];
        }
    }

    private static void CopyValues(GRASS.Face from, GRASS.Face to)
    {
        to.VertexIndexA = from.VertexIndexA;
        to.VertexIndexB = from.VertexIndexB;
        to.VertexIndexC = from.VertexIndexC;
        to.Normal = from.Normal;
    }
}
