using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The triangles of a Wavefront OBJ file, for the Collision Generator. Only positions and faces are read: texture
/// coordinates, normals, smoothing groups, lines and the material library are skipped, as collision doesn't use them.
/// <para>
/// The faces are split into groups, the OBJ equivalent of FLVER meshes: a group starts at each object (<c>o</c>),
/// group (<c>g</c>) or material (<c>usemtl</c>) line, so each group has one material. A material named
/// <c>hkm_Name_Property</c> gives its group that collision material, as FLVER material names do, which is how the
/// Model Editor's OBJ export names collision materials.
/// </para>
/// </summary>
public class ObjCollisionSource
{
    public class Group
    {
        /// <summary>The object or group name, or empty if the faces came before any</summary>
        public string Name = "";

        /// <summary>The usemtl material name, or empty if there was none</summary>
        public string Material = "";

        /// <summary>Indices into <see cref="Positions"/>, three per triangle</summary>
        public List<int> Triangles = new();
    }

    public List<Vector3> Positions = new();

    public List<Group> Groups = new();

    public static ObjCollisionSource Read(string path)
    {
        var source = new ObjCollisionSource();

        var name = "";
        var material = "";
        Group current = null;

        // Groups are found again by name and material, so faces that come back to an earlier group join it
        var groups = new Dictionary<(string Name, string Material), Group>();

        // Reused for every face, as faces can have any number of corners
        var corners = new List<int>();

        var lineNumber = 0;

        foreach (var rawLine in File.ReadLines(path))
        {
            lineNumber++;

            var line = rawLine.Trim();

            if (line.Length == 0 || line[0] == '#')
                continue;

            var split = line.IndexOfAny([' ', '\t']);
            var keyword = split < 0 ? line : line[..split];
            var rest = split < 0 ? "" : line[(split + 1)..].Trim();

            switch (keyword)
            {
                case "v":
                    source.Positions.Add(ParsePosition(rest, lineNumber));
                    break;

                case "o":
                case "g":
                    name = rest;
                    current = null;
                    break;

                case "usemtl":
                    material = rest;
                    current = null;
                    break;

                case "f":
                    if (current == null)
                    {
                        if (!groups.TryGetValue((name, material), out current))
                        {
                            current = new Group { Name = name, Material = material };
                            groups[(name, material)] = current;
                            source.Groups.Add(current);
                        }
                    }

                    ReadFace(source, rest, lineNumber, corners);

                    // Polygons are split into a fan of triangles around their first corner
                    for (var i = 1; i + 1 < corners.Count; i++)
                    {
                        current.Triangles.Add(corners[0]);
                        current.Triangles.Add(corners[i]);
                        current.Triangles.Add(corners[i + 1]);
                    }
                    break;

                // Everything else (vt, vn, vp, l, p, s, mtllib, ...) has nothing collision needs
                default:
                    break;
            }
        }

        source.Groups.RemoveAll(g => g.Triangles.Count == 0);

        return source;
    }

    /// <summary>
    /// The display name of a group in the Collision Generator's list
    /// </summary>
    public static string GetGroupName(Group group, int index)
    {
        return string.IsNullOrEmpty(group.Name) ? $"#{index}" : group.Name;
    }

    private static Vector3 ParsePosition(string text, int lineNumber)
    {
        // "v x y z", optionally followed by w or by a vertex color, which are ignored
        var parts = text.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3
            || !float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x)
            || !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y)
            || !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
        {
            throw new InvalidDataException(LOC.Get("HAVOK_CollisionGen_Obj_Invalid_Line", lineNumber, $"v {text}"));
        }

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Reads the position index of each corner of a face: "f v1 v2 v3", where each corner can also be v/vt, v//vn
    /// or v/vt/vn. Indices count from 1, or back from the latest position when negative.
    /// </summary>
    private static void ReadFace(ObjCollisionSource source, string text, int lineNumber, List<int> corners)
    {
        corners.Clear();

        foreach (var corner in text.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries))
        {
            var slash = corner.IndexOf('/');
            var positionText = slash < 0 ? corner : corner[..slash];

            if (!int.TryParse(positionText, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index) || index == 0)
            {
                throw new InvalidDataException(LOC.Get("HAVOK_CollisionGen_Obj_Invalid_Line", lineNumber, $"f {text}"));
            }

            // Relative indices refer to the positions read so far, so they are resolved as the face is read
            var resolved = index > 0 ? index - 1 : source.Positions.Count + index;

            if (resolved < 0 || resolved >= source.Positions.Count)
            {
                throw new InvalidDataException(LOC.Get("HAVOK_CollisionGen_Obj_Invalid_Index", lineNumber, index));
            }

            corners.Add(resolved);
        }

        if (corners.Count < 3)
        {
            throw new InvalidDataException(LOC.Get("HAVOK_CollisionGen_Obj_Invalid_Line", lineNumber, $"f {text}"));
        }
    }
}
