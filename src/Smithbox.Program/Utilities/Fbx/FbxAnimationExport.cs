#nullable enable
using StudioCore.Utilities.Interchange;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities.Fbx;

public sealed class FbxExportSettings
{
    /// <summary>How game space is mirrored into FBX's right-handed space</summary>
    public InterchangeHandedness Handedness = InterchangeHandedness.FlipZ;

    /// <summary>Root motion as a parent node of the skeleton (named <see cref="FbxAnimationExport.RootMotionNodeName" />)</summary>
    public bool RootMotionNode = true;

    public string Creator = "Smithbox";

    /// <summary>Skinned meshes written with the skeleton (F5), in game space; none by default</summary>
    public List<InterchangeMesh> Meshes = new();
}

/// <summary>What an export wrote, for the tool's report</summary>
public sealed class FbxExportResult
{
    public int Joints;
    public int AnimatedJoints;
    public int Frames;
    public double FrameRate;
    public int Curves;
    public bool RootMotion;
    public int Takes = 1;
    public int FloatTracks;
    public int Meshes;
    public int MeshVertices;
    public int MeshTriangles;
    public int Clusters;
}

/// <summary>
///     A skeleton and one clip as an FBX 7.4 scene: joints as <c>LimbNode</c> models with a bind pose, one animation
///     stack with a base layer, a curve per animated channel (Euler XYZ rotations, unrolled), Y up, metres
///     (<c>UnitScaleFactor</c> 100). Root motion is an animated <c>Null</c> above the skeleton's roots.
///     See Claude/Documentation/Animation Editor Analysis.md §14.3.
/// </summary>
public static class FbxAnimationExport
{
    /// <summary>FBX time units per second (KTime)</summary>
    public const long TicksPerSecond = 46186158000;

    public const string RootMotionNodeName = "RootMotion";

    // The FileId / CreationTime pair Blender writes, which the FBX SDK accepts as a valid file
    private static readonly byte[] FileId =
        { 0x28, 0xb3, 0x2a, 0xeb, 0xb6, 0x24, 0xcc, 0xc2, 0xbf, 0xc8, 0xb0, 0x2a, 0xa9, 0x2b, 0xfc, 0xf1 };

    private const string CreationTime = "1970-01-01 10:00:00:000";

    // KeyAttrFlags: linear interpolation (the clip is sampled per frame and played with lerp / slerp between them)
    private const int KeyLinear = 0x00000004;

    // KeyAttrDataFloat's third value: default weights, as written by the FBX SDK and Blender
    private static readonly float DefaultWeights = BitConverter.Int32BitsToSingle(0x0D050D05);

    /// <summary>The node whose animated user properties hold the clips' float tracks, one per float slot</summary>
    public const string FloatsNodeName = "Smithbox_Floats";

    public static FbxDocument Build(InterchangeSkeleton skeleton, InterchangeClip clip, FbxExportSettings settings,
        out FbxExportResult result) => Build(skeleton, new[] { clip }, settings, out result);

    /// <summary>
    ///     Several clips of one skeleton in one file: a take (AnimationStack) per clip, the first one current. Blender
    ///     reads each take as an action.
    /// </summary>
    public static FbxDocument Build(InterchangeSkeleton skeleton, IReadOnlyList<InterchangeClip> clips,
        FbxExportSettings settings, out FbxExportResult result)
    {
        if (clips.Count == 0 || clips.Any(c => c.FrameCount < 1))
            throw new ArgumentException("An FBX export needs clips with at least one frame each", nameof(clips));

        var b = new Builder(settings);
        result = b.Build(skeleton, clips);
        return b.Doc;
    }

    public static long FrameTicks(double frameRate) => (long)Math.Round(TicksPerSecond / frameRate);

    private sealed class Builder
    {
        public readonly FbxDocument Doc = new() { Version = FbxDocument.DefaultVersion };
        private readonly FbxExportSettings _settings;
        private readonly FbxNode _objects = new("Objects");
        private readonly FbxNode _connections = new("Connections");
        private readonly Dictionary<string, int> _counts = new();
        private long _nextId = 1_000_000;

        public Builder(FbxExportSettings settings)
        {
            _settings = settings;
        }

        private long NewId() => _nextId++;

        private void Count(string type) => _counts[type] = _counts.GetValueOrDefault(type) + 1;

        private void Connect(long child, long parent) =>
            _connections.Add("C", FbxProperty.String("OO"), FbxProperty.Int64(child), FbxProperty.Int64(parent));

        private void Connect(long child, long parent, string property) =>
            _connections.Add("C", FbxProperty.String("OP"), FbxProperty.Int64(child), FbxProperty.Int64(parent),
                FbxProperty.String(property));

        private InterchangeTransform Mirror(InterchangeTransform t) => InterchangeConvert.Mirror(t, _settings.Handedness);

        public FbxExportResult Build(InterchangeSkeleton skeleton, IReadOnlyList<InterchangeClip> clips)
        {
            var first = clips[0];
            var names = new List<string>();
            foreach (var c in clips)
            {
                var n = string.IsNullOrEmpty(c.Name) ? $"Take {names.Count + 1:D3}" : c.Name;
                while (names.Contains(n))
                    n += "_";
                names.Add(n);
            }

            var stops = clips.Select(c => Math.Max(c.FrameCount - 1, 0) * FrameTicks(c.FrameRate)).ToArray();
            var withRoot = _settings.RootMotionNode && clips.Any(c => c.RootMotion is { Length: > 0 });
            var floatNames = clips.SelectMany(c => c.Floats.Keys).Distinct().ToList();

            AddHeader(string.Join(" | ", clips.Select(c => c.Comment).Where(c => c.Length > 0)));
            AddGlobalSettings(first.FrameRate, stops.Max());
            AddDocuments(names[0]);
            Doc.Nodes.Add(new FbxNode("References"));

            // Models: the root motion null, then the joints
            var poseNodes = new List<(long Id, Matrix4x4 World)>();
            long rootId = 0;
            if (withRoot)
            {
                rootId = AddModel(RootMotionNodeName, "Null", InterchangeTransform.Identity);
                poseNodes.Add((rootId, Matrix4x4.Identity));
                Connect(rootId, 0);
            }

            var mirrored = new InterchangeSkeleton { Name = skeleton.Name };
            foreach (var j in skeleton.Joints)
                mirrored.Joints.Add(new InterchangeJoint { Name = j.Name, Parent = j.Parent, Rest = Mirror(j.Rest) });
            var restWorld = mirrored.RestWorld();

            var ids = new long[skeleton.Joints.Count];
            for (var j = 0; j < ids.Length; j++)
            {
                ids[j] = AddModel(mirrored.Joints[j].Name, "LimbNode", mirrored.Joints[j].Rest);
                poseNodes.Add((ids[j], restWorld[j]));
            }

            for (var j = 0; j < ids.Length; j++)
            {
                var parent = mirrored.Joints[j].Parent;
                Connect(ids[j], parent >= 0 ? ids[parent] : rootId);
            }

            // Float tracks: user properties on a null of their own, beside the skeleton
            long floatsId = 0;
            if (floatNames.Count > 0)
            {
                floatsId = AddModel(FloatsNodeName, "Null", InterchangeTransform.Identity, model =>
                {
                    var props = model.Child("Properties70")!;
                    foreach (var f in floatNames)
                    {
                        var start = clips.Select(c => c.Floats.GetValueOrDefault(f)).FirstOrDefault(v => v is { Length: > 0 });
                        props.Add(P(f, "Number", "", "A+U", FbxProperty.Double(start?[0] ?? 0)));
                    }
                });
                Connect(floatsId, 0);
            }

            // Skinned meshes (F5)
            var clusters = 0;
            foreach (var mesh in _settings.Meshes)
                clusters += AddMesh(mesh, ids, restWorld, poseNodes);

            AddBindPose(poseNodes);

            // Animation: a stack with a base layer per clip
            var curves = 0;
            var animated = 0;
            for (var c = 0; c < clips.Count; c++)
            {
                var clip = clips[c];
                var frameTicks = FrameTicks(clip.FrameRate);
                var stackId = NewId();
                Count("AnimationStack");
                var stackProps = new FbxNode("Properties70");
                stackProps.Add(P("LocalStart", "KTime", "Time", "", FbxProperty.Int64(0)));
                stackProps.Add(P("LocalStop", "KTime", "Time", "", FbxProperty.Int64(stops[c])));
                stackProps.Add(P("ReferenceStart", "KTime", "Time", "", FbxProperty.Int64(0)));
                stackProps.Add(P("ReferenceStop", "KTime", "Time", "", FbxProperty.Int64(stops[c])));
                _objects.Add("AnimationStack", FbxProperty.Int64(stackId), FbxProperty.ObjectName(names[c], "AnimStack"),
                    FbxProperty.String("")).Children.Add(stackProps);

                var layerId = NewId();
                Count("AnimationLayer");
                _objects.Add("AnimationLayer", FbxProperty.Int64(layerId), FbxProperty.ObjectName("BaseLayer", "AnimLayer"),
                    FbxProperty.String("")).EmptyBody = true;
                Connect(layerId, stackId);

                var times = Enumerable.Range(0, clip.FrameCount).Select(f => f * frameTicks).ToArray();
                for (var j = 0; j < ids.Length; j++)
                {
                    if (j >= clip.Tracks.Length || clip.Tracks[j] is not { } track)
                        continue;
                    if (c == 0)
                        animated++;
                    curves += AddChannels(ids[j], layerId, track.Select(Mirror).ToArray(), times);
                }

                if (withRoot && clip.RootMotion is { Length: > 0 } motion)
                    curves += AddChannels(rootId, layerId, motion.Select(Mirror).ToArray(), times);

                foreach (var (slot, values) in clip.Floats)
                {
                    if (values.Length == 0)
                        continue;
                    var node = NewId();
                    Count("AnimationCurveNode");
                    _objects.Add("AnimationCurveNode", FbxProperty.Int64(node), FbxProperty.ObjectName(slot, "AnimCurveNode"),
                        FbxProperty.String("")).Add("Properties70").Add(P("d|X", "Number", "", "A", FbxProperty.Double(values[0])));
                    Connect(node, layerId);
                    Connect(node, floatsId, slot);
                    AddCurve(node, "d|X", values, times);
                    curves++;
                }
            }

            Doc.Nodes.Add(Definitions());
            Doc.Nodes.Add(_objects);
            Doc.Nodes.Add(_connections);
            AddTakes(names, stops);

            return new FbxExportResult
            {
                Joints = ids.Length,
                AnimatedJoints = animated,
                Frames = first.FrameCount,
                FrameRate = first.FrameRate,
                Curves = curves,
                RootMotion = withRoot,
                Takes = clips.Count,
                FloatTracks = floatNames.Count,
                Meshes = _settings.Meshes.Count,
                MeshVertices = _settings.Meshes.Sum(m => m.VertexCount),
                MeshTriangles = _settings.Meshes.Sum(m => m.Triangles.Length / 3),
                Clusters = clusters,
            };
        }

        // ---------------------------------------------------------- sections

        private void AddHeader(string comment)
        {
            var now = DateTime.Now;
            var header = new FbxNode("FBXHeaderExtension");
            header.Add("FBXHeaderVersion", FbxProperty.Int32(1003));
            header.Add("FBXVersion", FbxProperty.Int32(Doc.Version));
            header.Add("EncryptionType", FbxProperty.Int32(0));
            var stamp = header.Add("CreationTimeStamp");
            stamp.Add("Version", FbxProperty.Int32(1000));
            stamp.Add("Year", FbxProperty.Int32(now.Year));
            stamp.Add("Month", FbxProperty.Int32(now.Month));
            stamp.Add("Day", FbxProperty.Int32(now.Day));
            stamp.Add("Hour", FbxProperty.Int32(now.Hour));
            stamp.Add("Minute", FbxProperty.Int32(now.Minute));
            stamp.Add("Second", FbxProperty.Int32(now.Second));
            stamp.Add("Millisecond", FbxProperty.Int32(now.Millisecond));
            header.Add("Creator", FbxProperty.String(_settings.Creator));

            var info = header.Add("SceneInfo", FbxProperty.ObjectName("GlobalInfo", "SceneInfo"),
                FbxProperty.String("UserData"));
            info.Add("Type", FbxProperty.String("UserData"));
            info.Add("Version", FbxProperty.Int32(100));
            var meta = info.Add("MetaData");
            meta.Add("Version", FbxProperty.Int32(100));
            foreach (var field in new[] { "Title", "Subject", "Author", "Keywords", "Revision" })
                meta.Add(field, FbxProperty.String(""));
            meta.Add("Comment", FbxProperty.String(comment));
            var props = info.Add("Properties70");
            props.Add(P("Original|ApplicationName", "KString", "", "", FbxProperty.String(_settings.Creator)));
            props.Add(P("LastSaved|ApplicationName", "KString", "", "", FbxProperty.String(_settings.Creator)));

            Doc.Nodes.Add(header);
            Doc.Nodes.Add(new FbxNode("FileId", FbxProperty.Raw(FileId)));
            Doc.Nodes.Add(new FbxNode("CreationTime", FbxProperty.String(CreationTime)));
            Doc.Nodes.Add(new FbxNode("Creator", FbxProperty.String(_settings.Creator)));
        }

        private void AddGlobalSettings(double frameRate, long stop)
        {
            var settings = new FbxNode("GlobalSettings");
            settings.Add("Version", FbxProperty.Int32(1000));
            var props = settings.Add("Properties70");

            // Y up, Z front, X across: right-handed, as Maya writes it
            props.Add(P("UpAxis", "int", "Integer", "", FbxProperty.Int32(1)));
            props.Add(P("UpAxisSign", "int", "Integer", "", FbxProperty.Int32(1)));
            props.Add(P("FrontAxis", "int", "Integer", "", FbxProperty.Int32(2)));
            props.Add(P("FrontAxisSign", "int", "Integer", "", FbxProperty.Int32(1)));
            props.Add(P("CoordAxis", "int", "Integer", "", FbxProperty.Int32(0)));
            props.Add(P("CoordAxisSign", "int", "Integer", "", FbxProperty.Int32(1)));
            props.Add(P("OriginalUpAxis", "int", "Integer", "", FbxProperty.Int32(1)));
            props.Add(P("OriginalUpAxisSign", "int", "Integer", "", FbxProperty.Int32(1)));

            // Values in metres: one unit is 100 cm
            props.Add(P("UnitScaleFactor", "double", "Number", "", FbxProperty.Double(100)));
            props.Add(P("OriginalUnitScaleFactor", "double", "Number", "", FbxProperty.Double(100)));

            props.Add(P("AmbientColor", "ColorRGB", "Color", "", FbxProperty.Double(0), FbxProperty.Double(0),
                FbxProperty.Double(0)));
            props.Add(P("DefaultCamera", "KString", "", "", FbxProperty.String("Producer Perspective")));
            props.Add(P("TimeMode", "enum", "", "", FbxProperty.Int32(TimeMode(frameRate))));
            props.Add(P("TimeProtocol", "enum", "", "", FbxProperty.Int32(2)));
            props.Add(P("SnapOnFrameMode", "enum", "", "", FbxProperty.Int32(0)));
            props.Add(P("TimeSpanStart", "KTime", "Time", "", FbxProperty.Int64(0)));
            props.Add(P("TimeSpanStop", "KTime", "Time", "", FbxProperty.Int64(stop)));
            props.Add(P("CustomFrameRate", "double", "Number", "", FbxProperty.Double(frameRate)));
            Doc.Nodes.Add(settings);
        }

        /// <summary>FbxTime::EMode for a frame rate: the named ones, else custom (14) with CustomFrameRate</summary>
        private static int TimeMode(double fps) => Math.Round(fps, 3) switch
        {
            120 => 1,
            100 => 2,
            60 => 3,
            50 => 4,
            48 => 5,
            30 => 6,
            24 => 11,
            _ => 14,
        };

        private void AddDocuments(string stackName)
        {
            var documents = new FbxNode("Documents");
            documents.Add("Count", FbxProperty.Int32(1));
            var document = documents.Add("Document", FbxProperty.Int64(NewId()), FbxProperty.String("Scene"),
                FbxProperty.String("Scene"));
            var props = document.Add("Properties70");
            props.Add(P("SourceObject", "object", "", ""));
            props.Add(P("ActiveAnimStackName", "KString", "", "", FbxProperty.String(stackName)));
            document.Add("RootNode", FbxProperty.Int64(0));
            Doc.Nodes.Add(documents);
        }

        private long AddModel(string name, string type, InterchangeTransform rest, Action<FbxNode>? extra = null)
        {
            // A mesh's attribute is its Geometry, connected by the caller
            long attributeId = 0;
            if (type != "Mesh")
            {
                attributeId = NewId();
                Count("NodeAttribute");
                var attribute = _objects.Add("NodeAttribute", FbxProperty.Int64(attributeId),
                    FbxProperty.ObjectName(name, "NodeAttribute"), FbxProperty.String(type));
                attribute.Add("TypeFlags", FbxProperty.String(type == "LimbNode" ? "Skeleton" : "Null"));
            }

            var id = NewId();
            Count("Model");
            var model = _objects.Add("Model", FbxProperty.Int64(id), FbxProperty.ObjectName(name, "Model"),
                FbxProperty.String(type));
            model.Add("Version", FbxProperty.Int32(232));
            var props = model.Add("Properties70");
            var euler = FbxEuler.ToXyzDegrees(rest.Rotation);
            props.Add(P("InheritType", "enum", "", "", FbxProperty.Int32(1)));
            props.Add(P("DefaultAttributeIndex", "int", "Integer", "", FbxProperty.Int32(0)));
            props.Add(P("Lcl Translation", "Lcl Translation", "", "A", Vec(rest.Translation)));
            props.Add(P("Lcl Rotation", "Lcl Rotation", "", "A", Vec(euler)));
            props.Add(P("Lcl Scaling", "Lcl Scaling", "", "A", Vec(rest.Scale)));
            model.Add("Shading", FbxProperty.Char((byte)'T'));
            model.Add("Culling", FbxProperty.String("CullingOff"));
            extra?.Invoke(model);

            if (attributeId != 0)
                Connect(attributeId, id);
            return id;
        }

        private readonly Dictionary<string, long> _materials = new();

        /// <summary>One material per name (no textures), shared by the meshes that use it</summary>
        private long MaterialFor(string name)
        {
            name = string.IsNullOrEmpty(name) ? "Material" : name;
            if (_materials.TryGetValue(name, out var known))
                return known;

            var id = NewId();
            Count("Material");
            var material = _objects.Add("Material", FbxProperty.Int64(id), FbxProperty.ObjectName(name, "Material"),
                FbxProperty.String(""));
            material.Add("Version", FbxProperty.Int32(102));
            material.Add("ShadingModel", FbxProperty.String("phong"));
            material.Add("MultiLayer", FbxProperty.Int32(0));
            material.Add("Properties70").Add(P("DiffuseColor", "Color", "", "A", FbxProperty.Double(0.8),
                FbxProperty.Double(0.8), FbxProperty.Double(0.8)));
            _materials[name] = id;
            return id;
        }

        // The handedness mirror as a matrix (its own inverse): a world matrix B becomes M · B · M
        private Matrix4x4 MirrorMatrix => _settings.Handedness == InterchangeHandedness.FlipX
            ? Matrix4x4.CreateScale(-1, 1, 1)
            : Matrix4x4.CreateScale(1, 1, -1);

        /// <summary>
        ///     A skinned mesh (F5): Geometry (positions, triangles in their own order, normals and
        ///     UVs per polygon vertex, one material), a Mesh model at the scene root, a Skin with a Cluster per joint
        ///     it uses (the vertices and weights, the joint's bind as TransformLink, its inverse as Transform: the mesh
        ///     sits at the origin). Returns the cluster count.
        /// </summary>
        public int AddMesh(InterchangeMesh mesh, long[] jointIds, Matrix4x4[] restWorld, List<(long Id, Matrix4x4 World)> poseNodes)
        {
            var mirror = MirrorMatrix;
            var count = mesh.VertexCount;

            var positions = new double[count * 3];
            for (var v = 0; v < count; v++)
            {
                var p = InterchangeConvert.Mirror(mesh.Positions[v], _settings.Handedness);
                positions[v * 3] = p.X;
                positions[v * 3 + 1] = p.Y;
                positions[v * 3 + 2] = p.Z;
            }

            // FLVER triangles are clockwise in game space (DirectX's front faces); the mirror makes them counter-
            // clockwise, FBX's front faces, so the order is kept (checked: every triangle faces its vertex normals)
            var polygon = new int[mesh.Triangles.Length];
            var corner = new int[mesh.Triangles.Length];
            for (var t = 0; t + 2 < mesh.Triangles.Length; t += 3)
            {
                corner[t] = mesh.Triangles[t];
                corner[t + 1] = mesh.Triangles[t + 1];
                corner[t + 2] = mesh.Triangles[t + 2];
                polygon[t] = corner[t];
                polygon[t + 1] = corner[t + 1];
                polygon[t + 2] = -(corner[t + 2] + 1);
            }

            var normals = new double[corner.Length * 3];
            for (var i = 0; i < corner.Length; i++)
            {
                var n = InterchangeConvert.Mirror(mesh.Normals[corner[i]], _settings.Handedness);
                normals[i * 3] = n.X;
                normals[i * 3 + 1] = n.Y;
                normals[i * 3 + 2] = n.Z;
            }

            var geometryId = NewId();
            Count("Geometry");
            var geometry = _objects.Add("Geometry", FbxProperty.Int64(geometryId),
                FbxProperty.ObjectName(mesh.Name, "Geometry"), FbxProperty.String("Mesh"));
            geometry.Add("Vertices", FbxProperty.DoubleArray(positions));
            geometry.Add("PolygonVertexIndex", FbxProperty.Int32Array(polygon));
            geometry.Add("GeometryVersion", FbxProperty.Int32(124));

            var normalLayer = geometry.Add("LayerElementNormal", FbxProperty.Int32(0));
            normalLayer.Add("Version", FbxProperty.Int32(101));
            normalLayer.Add("Name", FbxProperty.String(""));
            normalLayer.Add("MappingInformationType", FbxProperty.String("ByPolygonVertex"));
            normalLayer.Add("ReferenceInformationType", FbxProperty.String("Direct"));
            normalLayer.Add("Normals", FbxProperty.DoubleArray(normals));

            if (mesh.UVs != null)
            {
                var uvLayer = geometry.Add("LayerElementUV", FbxProperty.Int32(0));
                uvLayer.Add("Version", FbxProperty.Int32(101));
                uvLayer.Add("Name", FbxProperty.String("UVMap"));
                uvLayer.Add("MappingInformationType", FbxProperty.String("ByPolygonVertex"));
                uvLayer.Add("ReferenceInformationType", FbxProperty.String("IndexToDirect"));
                uvLayer.Add("UV", FbxProperty.DoubleArray(mesh.UVs.SelectMany(uv => new double[] { uv.X, uv.Y }).ToArray()));
                uvLayer.Add("UVIndex", FbxProperty.Int32Array(corner.ToArray()));
            }

            var materialLayer = geometry.Add("LayerElementMaterial", FbxProperty.Int32(0));
            materialLayer.Add("Version", FbxProperty.Int32(101));
            materialLayer.Add("Name", FbxProperty.String(""));
            materialLayer.Add("MappingInformationType", FbxProperty.String("AllSame"));
            materialLayer.Add("ReferenceInformationType", FbxProperty.String("IndexToDirect"));
            materialLayer.Add("Materials", FbxProperty.Int32Array(new[] { 0 }));

            var layer = geometry.Add("Layer", FbxProperty.Int32(0));
            layer.Add("Version", FbxProperty.Int32(100));
            foreach (var type in mesh.UVs != null
                         ? new[] { "LayerElementNormal", "LayerElementUV", "LayerElementMaterial" }
                         : new[] { "LayerElementNormal", "LayerElementMaterial" })
            {
                var element = layer.Add("LayerElement");
                element.Add("Type", FbxProperty.String(type));
                element.Add("TypedIndex", FbxProperty.Int32(0));
            }

            var modelId = AddModel(mesh.Name, "Mesh", InterchangeTransform.Identity);
            Connect(geometryId, modelId);
            Connect(modelId, 0);
            Connect(MaterialFor(mesh.Material), modelId);
            poseNodes.Add((modelId, Matrix4x4.Identity));

            // Skin: a cluster per joint with weight
            var skinId = NewId();
            Count("Deformer");
            var skin = _objects.Add("Deformer", FbxProperty.Int64(skinId), FbxProperty.ObjectName("", "Deformer"),
                FbxProperty.String("Skin"));
            skin.Add("Version", FbxProperty.Int32(101));
            skin.Add("Link_DeformAcuracy", FbxProperty.Double(50));
            Connect(skinId, geometryId);

            var byJoint = new SortedDictionary<int, (List<int> Indexes, List<double> Weights)>();
            for (var v = 0; v < count; v++)
            {
                for (var k = 0; k < 4; k++)
                {
                    var w = mesh.Weights[v * 4 + k];
                    var j = mesh.Joints[v * 4 + k];
                    if (w <= 0 || j < 0 || j >= jointIds.Length)
                        continue;
                    if (!byJoint.TryGetValue(j, out var list))
                        byJoint[j] = list = (new List<int>(), new List<double>());
                    list.Indexes.Add(v);
                    list.Weights.Add(w);
                }
            }

            foreach (var (j, (indexes, weights)) in byJoint)
            {
                var link = j < mesh.JointBind.Length && mesh.JointBind[j] is { } b ? mirror * b * mirror : restWorld[j];
                Matrix4x4.Invert(link, out var transform);

                var clusterId = NewId();
                Count("Deformer");
                var cluster = _objects.Add("Deformer", FbxProperty.Int64(clusterId),
                    FbxProperty.ObjectName("", "SubDeformer"), FbxProperty.String("Cluster"));
                cluster.Add("Version", FbxProperty.Int32(100));
                cluster.Add("UserData", FbxProperty.String(""), FbxProperty.String(""));
                cluster.Add("Indexes", FbxProperty.Int32Array(indexes.ToArray()));
                cluster.Add("Weights", FbxProperty.DoubleArray(weights.ToArray()));
                cluster.Add("Transform", FbxProperty.DoubleArray(MatrixArray(transform)));
                cluster.Add("TransformLink", FbxProperty.DoubleArray(MatrixArray(link)));
                Connect(clusterId, skinId);
                Connect(jointIds[j], clusterId);
            }

            return byJoint.Count;
        }

        private static double[] MatrixArray(Matrix4x4 m) => new double[]
        {
            m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24,
            m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44,
        };

        private void AddBindPose(List<(long Id, Matrix4x4 World)> nodes)
        {
            Count("Pose");
            var pose = _objects.Add("Pose", FbxProperty.Int64(NewId()), FbxProperty.ObjectName("BIND_POSES", "Pose"),
                FbxProperty.String("BindPose"));
            pose.Add("Type", FbxProperty.String("BindPose"));
            pose.Add("Version", FbxProperty.Int32(100));
            pose.Add("NbPoseNodes", FbxProperty.Int32(nodes.Count));
            foreach (var (id, m) in nodes)
            {
                var node = pose.Add("PoseNode");
                node.Add("Node", FbxProperty.Int64(id));

                // FbxAMatrix memory order: row-vector rows, translation last (the System.Numerics field order)
                node.Add("Matrix", FbxProperty.DoubleArray(new double[]
                {
                    m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24,
                    m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44,
                }));
            }
        }

        /// <summary>Translation, rotation and (when it changes from 1) scale curves of one model; returns the curve count.</summary>
        private int AddChannels(long model, long layer, InterchangeTransform[] frames, long[] times)
        {
            var count = frames.Length;
            var translation = new Vector3[count];
            var rotation = new Vector3[count];
            var scale = new Vector3[count];
            var previous = Vector3.Zero;
            for (var f = 0; f < count; f++)
            {
                translation[f] = frames[f].Translation;
                rotation[f] = f == 0
                    ? FbxEuler.ToXyzDegrees(frames[f].Rotation)
                    : FbxEuler.ToXyzDegrees(frames[f].Rotation, previous);
                previous = rotation[f];
                scale[f] = frames[f].Scale;
            }

            var curves = AddCurveNode(model, layer, "T", "Lcl Translation", translation, times);
            curves += AddCurveNode(model, layer, "R", "Lcl Rotation", rotation, times);
            if (scale.Any(s => Vector3.DistanceSquared(s, Vector3.One) > 1e-12f))
                curves += AddCurveNode(model, layer, "S", "Lcl Scaling", scale, times);
            return curves;
        }

        private int AddCurveNode(long model, long layer, string name, string property, Vector3[] values, long[] times)
        {
            var id = NewId();
            Count("AnimationCurveNode");
            var node = _objects.Add("AnimationCurveNode", FbxProperty.Int64(id),
                FbxProperty.ObjectName(name, "AnimCurveNode"), FbxProperty.String(""));
            var props = node.Add("Properties70");
            var first = values.Length > 0 ? values[0] : Vector3.Zero;
            props.Add(P("d|X", "Number", "", "A", FbxProperty.Double(first.X)));
            props.Add(P("d|Y", "Number", "", "A", FbxProperty.Double(first.Y)));
            props.Add(P("d|Z", "Number", "", "A", FbxProperty.Double(first.Z)));
            Connect(id, layer);
            Connect(id, model, property);

            AddCurve(id, "d|X", values.Select(v => v.X).ToArray(), times);
            AddCurve(id, "d|Y", values.Select(v => v.Y).ToArray(), times);
            AddCurve(id, "d|Z", values.Select(v => v.Z).ToArray(), times);
            return 3;
        }

        private void AddCurve(long curveNode, string channel, float[] values, long[] times)
        {
            // A channel that doesn't change is one key: lossless, and much smaller for the many still channels
            var constant = values.All(v => v == values[0]);
            var keyTimes = constant ? new[] { times[0] } : times;
            var keyValues = constant ? new[] { values[0] } : values;

            var id = NewId();
            Count("AnimationCurve");
            var curve = _objects.Add("AnimationCurve", FbxProperty.Int64(id), FbxProperty.ObjectName("", "AnimCurve"),
                FbxProperty.String(""));
            curve.Add("Default", FbxProperty.Double(values[0]));
            curve.Add("KeyVer", FbxProperty.Int32(4008));
            curve.Add("KeyTime", FbxProperty.Int64Array(keyTimes));
            curve.Add("KeyValueFloat", FbxProperty.FloatArray(keyValues));
            curve.Add("KeyAttrFlags", FbxProperty.Int32Array(new[] { KeyLinear }));
            curve.Add("KeyAttrDataFloat", FbxProperty.FloatArray(new[] { 0f, 0f, DefaultWeights, 0f }));
            curve.Add("KeyAttrRefCount", FbxProperty.Int32Array(new[] { keyTimes.Length }));
            Connect(id, curveNode, channel);
        }

        private FbxNode Definitions()
        {
            var definitions = new FbxNode("Definitions");
            definitions.Add("Version", FbxProperty.Int32(100));
            var types = new List<(string, int)> { ("GlobalSettings", 1) };
            types.AddRange(_counts.Select(c => (c.Key, c.Value)));
            definitions.Add("Count", FbxProperty.Int32(types.Sum(t => t.Item2)));
            foreach (var (type, count) in types)
                definitions.Add("ObjectType", FbxProperty.String(type)).Add("Count", FbxProperty.Int32(count));
            return definitions;
        }

        private void AddTakes(IReadOnlyList<string> names, long[] stops)
        {
            var takes = new FbxNode("Takes");
            takes.Add("Current", FbxProperty.String(names[0]));
            for (var i = 0; i < names.Count; i++)
            {
                var take = takes.Add("Take", FbxProperty.String(names[i]));
                take.Add("FileName", FbxProperty.String(names[i] + ".tak"));
                take.Add("LocalTime", FbxProperty.Int64(0), FbxProperty.Int64(stops[i]));
                take.Add("ReferenceTime", FbxProperty.Int64(0), FbxProperty.Int64(stops[i]));
            }

            Doc.Nodes.Add(takes);
        }

        private static FbxNode P(string name, string type, string label, string flags, params FbxProperty[] values)
        {
            var p = new FbxNode("P", FbxProperty.String(name), FbxProperty.String(type), FbxProperty.String(label),
                FbxProperty.String(flags));
            p.Properties.AddRange(values);
            return p;
        }

        private static FbxProperty[] Vec(Vector3 v) =>
            new[] { FbxProperty.Double(v.X), FbxProperty.Double(v.Y), FbxProperty.Double(v.Z) };
    }
}
