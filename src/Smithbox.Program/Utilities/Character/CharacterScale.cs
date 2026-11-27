#nullable enable
using Andre.IO.VFS;
using Havok;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Interface;
using StudioCore.Renderer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Utilities;

/// <summary>What a character scale writes (Character Scale Plan.md): each part can be left out</summary>
public sealed class CharacterScaleSettings
{
    public float Scale = 1f;

    /// <summary>Every FLVER in the chrbnd: vertices, bones, dummies, bounding boxes</summary>
    public bool Models = true;

    /// <summary>The anibnd skeleton's reference pose</summary>
    public bool Skeleton = true;

    /// <summary>Every clip's translation tracks and root motion</summary>
    public bool Clips = true;

    /// <summary>The behavior character file(s): foot and hand IK, the controller capsule (DS3, BB)</summary>
    public bool Behavior = true;

    /// <summary>The time acts' distance fields (event 760's approach distances, ER / NR ground offsets)</summary>
    public bool TimeActs = true;

    /// <summary>Any linked param group is on (applied by the caller: <see cref="CharacterParamScale" />)</summary>
    public bool LinkedParams = true;

    /// <summary>The chrbnd's physics file(s): the ragdoll (AC6: also a physics system)</summary>
    public bool Ragdoll = true;

    /// <summary>The chrbnd's cloth file(s)</summary>
    public bool Cloth = true;

    /// <summary>D3: physics mass grows with volume (s³, inertia s⁵); off keeps it (inertia s²)</summary>
    public bool ScaleMass = true;

    /// <summary>The NpcParam body size fields of every row using the model (applied by the caller: see
    /// <see cref="CharacterScale.PlanNpcParamEdits" />)</summary>
    public bool BodyParams = true;

    /// <summary>Clips re-encoded on several threads</summary>
    public bool Parallel = true;
}

/// <summary>The files a character scale reads and writes, and why it can't go ahead (if so)</summary>
public sealed class CharacterScalePlan
{
    public ProjectType Game;
    public string Character = "";
    public CharacterScaleSettings Settings = new();

    /// <summary>The chrbnd(s) holding the character's FLVERs</summary>
    public List<string> ModelBinders = new();

    /// <summary>The character's anibnds, the base one (with the skeleton) first</summary>
    public List<string> AnimationBinders = new();

    /// <summary>Binders holding behavior character files (characters/cXXXX.hkx): the behbnd, or the anibnd in BB</summary>
    public List<string> BehaviorBinders = new();

    public int ModelCount;
    public int PhysicsFileCount;
    public int ClothFileCount;
    public int ClipCount;
    public int BehaviorFileCount;

    /// <summary>Binders holding time acts (.tae): the anibnds, AC6's behbnd</summary>
    public List<string> TimeActBinders = new();

    public int TimeActFileCount;

    /// <summary>The character has its own skeleton but plays another character's clips</summary>
    public bool BorrowsClips;

    /// <summary>Characters that play this character's clips (NpcParam normalChangeAnimChrId), from the caller</summary>
    public List<string> SharedWith = new();

    /// <summary>Why the scale can't be applied as it is; empty when it can</summary>
    public List<string> Refusals = new();

    /// <summary>What the scale does not change yet, for the prompt</summary>
    public List<string> NotScaled = new();

    public bool CanApply => Refusals.Count == 0;
}

public sealed class CharacterScaleResult
{
    public List<string> Written = new();
    public List<string> Failures = new();

    /// <summary>Stopped by its cancellation token: the binders before were written, the rest weren't</summary>
    public bool Cancelled;
    public int Models;
    public int Vertices;
    public int SkeletonBones;
    public int Clips;

    /// <summary>Clips scaled exactly (their stored ranges and values × s); the rest were encoded again</summary>
    public int ClipsExact;

    public int ClipsFailed;

    /// <summary>Physics files scaled, and the values changed in them</summary>
    public int PhysicsFiles;
    public int PhysicsValues;

    /// <summary>Cloth files scaled, and the values changed in them</summary>
    public int ClothFiles;
    public int ClothValues;

    /// <summary>Behavior character files scaled, and the values changed in them</summary>
    public int BehaviorFiles;
    public int BehaviorValues;

    /// <summary>Time act files scaled, and the event values changed in them</summary>
    public int TimeActFiles;
    public int TimeActValues;

    /// <summary>Behavior character files that couldn't be read as Havok (left as they are)</summary>
    public List<string> BehaviorSkipped = new();

    /// <summary>The worst re-encoded clip's error over its tolerance (≤ 1 holds tolerance)</summary>
    public double WorstErrorRatio;

    public string? WorstClip;
}

/// <summary>
///     Scales a character uniformly by rewriting every length in its files (Character Scale Plan.md, D1: bake only).
///     P1: the FLVERs, the anibnd skeleton and every clip. No UI: <see cref="CreatePlan" /> then <see cref="Apply" />,
///     which writes through a delegate (the project's file system in the app, a folder in the offline checks).
/// </summary>
public static class CharacterScale
{
    /// <summary>The games whose clips Smithbox can write (hk2014 to hk2019)</summary>
    public static bool IsSupportedGame(ProjectType game) =>
        game is ProjectType.DS3 or ProjectType.BB or ProjectType.SDT or ProjectType.ER or ProjectType.AC6 or ProjectType.NR;

    /// <summary>The anibnd skeleton fields (§4.1)</summary>
    public static readonly HavokScaleRule[] SkeletonRules =
    {
        new("hkaSkeleton", "m_referencePose[].m_translation", HavokScaleOp.Xyz)
    };

    /// <summary>The hknp convex shapes (every game's ragdolls, AC6's physics systems)</summary>
    private static readonly string[] ConvexShapes =
    {
        "hknpCapsuleShape", "hknpConvexPolytopeShape", "hknpBoxShape", "hknpCylinderShape", "hknpSphereShape",
        "hknpConvexShape"
    };

    /// <summary>
    ///     The chrbnd physics file's fields (§4.1, §4.5): its skeletons and mapper, body positions, shapes (convex,
    ///     compressed mesh), mass properties, drag and constraint pivots. Mass with volume (s³, inertia s⁵) when
    ///     <paramref name="scaleMass" /> (D3), else kept (inertia s²: the same mass over a larger body).
    /// </summary>
    public static List<HavokScaleRule> PhysicsRules(bool scaleMass)
    {
        var mass = scaleMass ? 3 : 0;
        var inertia = scaleMass ? 5 : 2;
        var rules = new List<HavokScaleRule>(SkeletonRules)
        {
            new("hkaSkeletonMapper", "m_mapping.m_simpleMappings[].m_aFromBTransform.m_translation", HavokScaleOp.Xyz),
            new("hkaSkeletonMapper", "m_mapping.m_chainMappings[].m_startAFromBTransform.m_translation", HavokScaleOp.Xyz),
            new("hkaSkeletonMapper", "m_mapping.m_chainMappings[].m_endAFromBTransform.m_translation", HavokScaleOp.Xyz),
            new("hkaSkeletonMapper", "m_mapping.m_extractedMotionMapping.m_translation", HavokScaleOp.Xyz),
            new("hkSimpleLocalFrame", "m_transform", HavokScaleOp.Translation),

            // Compressed meshes: every stored float is a length and the rest is quantized relative to them
            new("hknpCompressedMeshShape", "m_convexRadius", HavokScaleOp.Length),
            new("hknpCompressedMeshShape", "m_externShapes[].m_transform", HavokScaleOp.Translation),
            new("hknpCompressedMeshShapeData", "m_meshTree.m_domain.m_min", HavokScaleOp.Xyz),
            new("hknpCompressedMeshShapeData", "m_meshTree.m_domain.m_max", HavokScaleOp.Xyz),
            new("hknpCompressedMeshShapeData", "m_meshTree.m_sections[].m_codecParms[]", HavokScaleOp.Length),
            new("hknpCompressedMeshShapeData", "m_meshTree.m_sections[].m_domain.m_min", HavokScaleOp.Xyz),
            new("hknpCompressedMeshShapeData", "m_meshTree.m_sections[].m_domain.m_max", HavokScaleOp.Xyz),

            // Mass: per shape (packed centre and inertia) and per distribution
            new("hknpShapeMassProperties", "m_compressedMassProperties.m_mass", HavokScaleOp.Length, mass),
            new("hknpShapeMassProperties", "m_compressedMassProperties.m_volume", HavokScaleOp.Length, 3),
            new("hknpShapeMassProperties", "m_compressedMassProperties.m_centerOfMass", HavokScaleOp.Packed),
            new("hknpShapeMassProperties", "m_compressedMassProperties.m_inertia", HavokScaleOp.Packed, inertia),
            new("hknpRefMassDistribution", "m_massDistribution.m_centerOfMassAndVolume", HavokScaleOp.Xyz),
            new("hknpRefMassDistribution", "m_massDistribution.m_centerOfMassAndVolume", HavokScaleOp.W, 3),
            new("hknpRefMassDistribution", "m_massDistribution.m_inertiaTensor", HavokScaleOp.Xyz, inertia),

            // Drag: centres and offsets are lengths, the W of an angular effect an area (fitted on 8909 ER capsules)
            new("hknpRefDragProperties", "m_dragProperties.m_centerAndOffset[]", HavokScaleOp.Xyzw),
            new("hknpRefDragProperties", "m_dragProperties.m_angularEffectsAndArea[]", HavokScaleOp.W, 2),

            // Constraint pivots (the ball-and-socket pivot's W mirrors a coordinate)
            new("hkpBallAndSocketConstraintData", "m_atoms.m_pivots.m_translationA", HavokScaleOp.Xyzw),
            new("hkpBallAndSocketConstraintData", "m_atoms.m_pivots.m_translationB", HavokScaleOp.Xyzw)
        };

        foreach (var system in new[] { "hknpRagdollData", "hknpPhysicsSystemData" })
        {
            rules.Add(new(system, "m_bodyCinfos[].m_position", HavokScaleOp.Xyz));
            rules.Add(new(system, "m_bodyCinfos[].m_collisionLookAheadDistance", HavokScaleOp.Length));
            rules.Add(new(system, "m_bodyCinfos[].m_mass", HavokScaleOp.Length, mass));
        }

        foreach (var constraint in new[] { "hkpRagdollConstraintData", "hkpLimitedHingeConstraintData",
                     "hkpHingeConstraintData", "hkpFixedConstraintData" })
        {
            rules.Add(new(constraint, "m_atoms.m_transforms.m_transformA", HavokScaleOp.Translation));
            rules.Add(new(constraint, "m_atoms.m_transforms.m_transformB", HavokScaleOp.Translation));
        }

        foreach (var shape in ConvexShapes)
        {
            rules.Add(new(shape, "m_convexRadius", HavokScaleOp.Length));
            rules.Add(new(shape, "m_maxAllowedPenetration", HavokScaleOp.Length));
            // A vertex's W is 0.5 plus its index bits: kept
            rules.Add(new(shape, "m_vertices[]", HavokScaleOp.Xyz));
            rules.Add(new(shape, "m_planes[]", HavokScaleOp.W));
            // AC6's hknpConvexShape keeps its hull apart
            rules.Add(new(shape, "m_hull.m_vertices[].m_x", HavokScaleOp.Length));
            rules.Add(new(shape, "m_hull.m_vertices[].m_y", HavokScaleOp.Length));
            rules.Add(new(shape, "m_hull.m_vertices[].m_z", HavokScaleOp.Length));
            rules.Add(new(shape, "m_hull.m_planes[]", HavokScaleOp.W));
        }

        // Capsule and cylinder ends; A's W is the radius, B's is 0
        foreach (var shape in new[] { "hknpCapsuleShape", "hknpCylinderShape" })
        {
            rules.Add(new(shape, "m_a", HavokScaleOp.Xyzw));
            rules.Add(new(shape, "m_b", HavokScaleOp.Xyz));
        }

        rules.Add(new("hknpBoxShape", "m_obb", HavokScaleOp.Obb));
        foreach (var lane in SimdLanes)
            rules.Add(new("hknpCompressedMeshShapeData", $"m_simdTree.m_nodes[].{lane}", HavokScaleOp.Xyzw));
        return rules;
    }

    /// <summary>
    ///     The cloth file's plain fields (§4.6): skeleton, rest pose, particle radii, link lengths, local range limits,
    ///     volume frames, bend curvature, collision shapes and transforms, skin bind matrices. Particle masses and the
    ///     mass-weighted link stiffnesses stay (stiffness × reduced mass is one user value per set; bend stiffness has
    ///     no power of length), so cloth keeps its mass whatever D3 says. Frames and deformers:
    ///     <see cref="ClothFrame.ScaleOperators" />.
    /// </summary>
    public static readonly List<HavokScaleRule> ClothRules = new(SkeletonRules)
    {
        new("hclSimClothPose", "m_positions[]", HavokScaleOp.Xyz),
        new("hclSimClothData", "m_particleDatas[].m_radius", HavokScaleOp.Length),
        new("hclSimClothData", "m_maxParticleRadius", HavokScaleOp.Length),
        new("hclSimClothData", "m_collidablePinchingDatas[].m_pinchDetectionRadius", HavokScaleOp.Length),
        new("hclSimClothData", "m_landscapeCollisionData.m_landscapeRadius", HavokScaleOp.Length),
        new("hclSimClothData", "m_landscapeCollisionData.m_collisionTolerance", HavokScaleOp.Length),
        new("hclSimClothData", "m_landscapeCollisionData.m_pinchDetectionRadius", HavokScaleOp.Length),
        new("hclSimClothData", "m_collidableTransformMap.m_offsets[]", HavokScaleOp.Translation),

        new("hclStandardLinkConstraintSet", "m_links[].m_restLength", HavokScaleOp.Length),
        new("hclStretchLinkConstraintSet", "m_links[].m_restLength", HavokScaleOp.Length),
        new("hclCompressibleLinkConstraintSet", "m_links[].m_restLength", HavokScaleOp.Length),
        new("hclCompressibleLinkConstraintSet", "m_links[].m_compressionLength", HavokScaleOp.Length),
        new("hclBendLinkConstraintSet", "m_links[].m_bendMinLength", HavokScaleOp.Length),
        new("hclBendLinkConstraintSet", "m_links[].m_stretchMaxLength", HavokScaleOp.Length),
        new("hclBendStiffnessConstraintSet", "m_links[].m_restCurvature", HavokScaleOp.Length, -1),
        new("hclBendStiffnessConstraintSet", "m_maxRestPoseHeightSq", HavokScaleOp.Length, 2),
        new("hclLocalRangeConstraintSet", "m_localConstraints[].m_maximumDistance", HavokScaleOp.Length),
        new("hclLocalRangeConstraintSet", "m_localConstraints[].m_minNormalDistance", HavokScaleOp.Length),
        new("hclLocalRangeConstraintSet", "m_localConstraints[].m_maxNormalDistance", HavokScaleOp.Length),
        new("hclVolumeConstraintMx", "m_frameBatchDatas[].m_frameVector[]", HavokScaleOp.Xyz),
        new("hclVolumeConstraintMx", "m_frameSingleDatas[].m_frameVector", HavokScaleOp.Xyz),
        new("hclVolumeConstraintMx", "m_applyBatchDatas[].m_frameVector[]", HavokScaleOp.Xyz),
        new("hclVolumeConstraintMx", "m_applySingleDatas[].m_frameVector", HavokScaleOp.Xyz),
        new("hclVolumeConstraint", "m_frameDatas[].m_frameVector", HavokScaleOp.Xyz),
        new("hclVolumeConstraint", "m_applyDatas[].m_frameVector", HavokScaleOp.Xyz),

        new("hclCollidable", "m_transform", HavokScaleOp.Translation),
        new("hclCollidable", "m_pinchDetectionRadius", HavokScaleOp.Length),
        new("hclCapsuleShape", "m_start", HavokScaleOp.Xyz),
        new("hclCapsuleShape", "m_end", HavokScaleOp.Xyz),
        new("hclCapsuleShape", "m_radius", HavokScaleOp.Length),
        new("hclCapsuleShape", "m_capLenSqrdInv", HavokScaleOp.Length, -2),
        new("hclTaperedCapsuleShape", "m_small", HavokScaleOp.Xyz),
        new("hclTaperedCapsuleShape", "m_big", HavokScaleOp.Xyz),
        new("hclTaperedCapsuleShape", "m_coneApex", HavokScaleOp.Xyz),
        new("hclTaperedCapsuleShape", "m_smallRadius", HavokScaleOp.Length),
        new("hclTaperedCapsuleShape", "m_bigRadius", HavokScaleOp.Length),
        new("hclTaperedCapsuleShape", "m_d", HavokScaleOp.Length),
        new("hclTaperedCapsuleShape", "m_l", HavokScaleOp.Length),
        new("hclTaperedCapsuleShape", "m_dVec", HavokScaleOp.Xyzw),
        new("hclTaperedCapsuleShape", "m_lVec", HavokScaleOp.Xyzw),
        new("hclSphereShape", "m_sphere.m_pos", HavokScaleOp.Xyzw),
        new("hclPlaneShape", "m_planeEquation", HavokScaleOp.W),
        new("hclConvexGeometryShape", "m_geomCentroid", HavokScaleOp.Xyz),
        new("hclConvexGeometryShape", "m_objAabb.m_min", HavokScaleOp.Xyz),
        new("hclConvexGeometryShape", "m_objAabb.m_max", HavokScaleOp.Xyz),
        new("hclConvexGeometryShape", "m_tetrahedraEquations[]", HavokScaleOp.Row4),
        new("hclConvexGeometryShape", "m_invCellSize", HavokScaleOp.Xyz, -1),
        new("hclConvexGeometryShape", "m_localFromWorld", HavokScaleOp.Translation),
        new("hclConvexGeometryShape", "m_worldFromLocal", HavokScaleOp.Translation),

        new("hclObjectSpaceSkinPOperator", "m_boneFromSkinMeshTransforms[]", HavokScaleOp.Translation),
        new("hclObjectSpaceSkinPNOperator", "m_boneFromSkinMeshTransforms[]", HavokScaleOp.Translation),
        new("hclObjectSpaceSkinPNTOperator", "m_boneFromSkinMeshTransforms[]", HavokScaleOp.Translation),
        new("hclObjectSpaceSkinPNTBOperator", "m_boneFromSkinMeshTransforms[]", HavokScaleOp.Translation)
    };

    /// <summary>The Assets folder the templates are read from; null: the one beside the program (offline tools set it)</summary>
    public static string? AssetsRoot;

    /// <summary>
    ///     The time act event fields that are distances (§4.7), by their names in each game's template, so an event ID
    ///     that means something else in another game (604 is SetGroundOffset only in ER and NR) is left
    /// </summary>
    public static readonly string[] TimeActFields =
    {
        "ReferenceDist", "EnableRangeMin", "EnableRangeMax", "ArriveDistFromTarget",
        "GroundOffsetAtEventStart", "GroundOffsetAtEventEnd"
    };

    private static readonly Dictionary<ProjectType, Dictionary<int, int[]>> TimeActOffsets = new();

    /// <summary>Event ID → byte offsets of its distance fields, read from Assets/TAE/&lt;game&gt;/Defs/Character.xml</summary>
    public static Dictionary<int, int[]> TimeActDistanceOffsets(ProjectType game, string? templatePath = null)
    {
        lock (TimeActOffsets)
        {
            if (templatePath == null && TimeActOffsets.TryGetValue(game, out var known))
                return known;

            var result = new Dictionary<int, int[]>();
            var path = templatePath ?? Path.Join(AssetsRoot ?? Path.Join(AppContext.BaseDirectory, "Assets"), "TAE",
                StudioCore.Application.ProjectUtils.GetGameDirectory(game), "Defs", "Character.xml");
            if (File.Exists(path))
            {
                foreach (var action in System.Xml.Linq.XDocument.Load(path).Root?.Elements("action") ??
                                       Enumerable.Empty<System.Xml.Linq.XElement>())
                {
                    if (!int.TryParse(action.Attribute("id")?.Value, out var id))
                        continue;
                    var offset = 0;
                    var found = new List<int>();
                    foreach (var p in action.Elements())
                    {
                        var size = p.Name.LocalName switch
                        {
                            "b" or "u8" or "s8" => 1,
                            "u16" or "s16" => 2,
                            "u32" or "s32" or "f32" => 4,
                            "u64" or "s64" or "f64" or "f32grad" => 8,
                            "aob" => int.TryParse(p.Attribute("length")?.Value, out var n) ? n : -1,
                            _ => -1
                        };
                        if (size < 0)
                        {
                            found.Clear();
                            break;
                        }
                        if (p.Name.LocalName == "f32" && TimeActFields.Contains(p.Attribute("name")?.Value))
                            found.Add(offset);
                        offset += size;
                    }
                    if (found.Count > 0)
                        result[id] = found.ToArray();
                }
            }

            if (templatePath == null)
                TimeActOffsets[game] = result;
            return result;
        }
    }

    /// <summary>The distance fields of every event of a binder's time acts × s. Returns the values changed.</summary>
    private static int ScaleTimeActFiles(CharacterScalePlan plan, IBinder bnd, CharacterScaleResult result)
    {
        var offsets = TimeActDistanceOffsets(plan.Game);
        if (offsets.Count == 0)
            return 0;

        var changed = 0;
        foreach (var file in bnd.Files.Where(f => f.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase)))
        {
            var (raw, dcx) = Unpack(file.Bytes);
            var tae = TAE.Read(raw);
            var values = 0;
            foreach (var anim in tae.Animations)
                foreach (var ev in anim.Events)
                {
                    if (!offsets.TryGetValue(ev.Type, out var at))
                        continue;
                    var bytes = (byte[])ev.GetParameterBytes(tae.BigEndian).Clone();
                    foreach (var o in at)
                    {
                        if (o + 4 > bytes.Length)
                            continue;
                        var span = bytes.AsSpan(o, 4);
                        var v = tae.BigEndian
                            ? BitConverter.Int32BitsToSingle(System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(span))
                            : BitConverter.ToSingle(span);
                        var scaled = HavokFieldScale.Mul(v, plan.Settings.Scale);
                        if (scaled == v)
                            continue;
                        if (tae.BigEndian)
                            System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(span, BitConverter.SingleToInt32Bits(scaled));
                        else
                            BitConverter.TryWriteBytes(span, scaled);
                        values++;
                    }
                    ev.SetParameterBytes(tae.BigEndian, bytes, true);
                }

            if (values == 0)
                continue;
            file.Bytes = Pack(tae.Write(), dcx);
            result.TimeActFiles++;
            result.TimeActValues += values;
            changed += values;
        }

        return changed;
    }

    /// <summary>A chrbnd cloth file: cXXXX_c.hkx (700), BB's cXXXX_cloth.hkx (1000)</summary>
    public static bool IsClothFile(BinderFile file) =>
        file.Name.EndsWith("_c.hkx", StringComparison.OrdinalIgnoreCase) ||
        file.Name.EndsWith("_cloth.hkx", StringComparison.OrdinalIgnoreCase);

    /// <summary>The SIMD tree's lane bounds (hk2018, hk2019 compressed meshes)</summary>
    private static readonly string[] SimdLanes = { "m_lx", "m_hx", "m_ly", "m_hy", "m_lz", "m_hz" };

    /// <summary>A chrbnd physics file: binder ID 3xx (cXXXX.hkx, AC6's cXXXX_1.hkx)</summary>
    public static bool IsPhysicsFile(BinderFile file) =>
        file.ID / 100 == 3 && file.Name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase);

    /// <summary>The behavior character file fields (§4.1, §4.2): foot / hand IK and the DS3 / BB controller capsule</summary>
    public static readonly HavokScaleRule[] BehaviorRules =
    {
        new("hkbFootIkDriverInfo", "m_legs[].m_footEndLS", HavokScaleOp.Xyz),
        new("hkbFootIkDriverInfo", "m_legs[].m_footPlantedAnkleHeightMS", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_legs[].m_footRaisedAnkleHeightMS", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_legs[].m_maxAnkleHeightMS", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_legs[].m_minAnkleHeightMS", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_legs[].m_heelOffsetFromAnkle", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_raycastDistanceUp", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_raycastDistanceDown", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_originalGroundHeightMS", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_verticalOffset", HavokScaleOp.Length),
        new("hkbFootIkDriverInfo", "m_sidewaysSampleWidth", HavokScaleOp.Length),
        new("hkbHandIkDriverInfo", "m_hands[].m_handOffsetLS", HavokScaleOp.Xyz),
        new("hkbCharacterData", "m_characterControllerSetup.m_rigidBodySetup.m_shapeSetup.m_capsuleHeight", HavokScaleOp.Length),
        new("hkbCharacterData", "m_characterControllerSetup.m_rigidBodySetup.m_shapeSetup.m_capsuleRadius", HavokScaleOp.Length),
        new("hkbCharacterData", "m_boneAttachmentTransforms[]", HavokScaleOp.Translation)
    };

    /// <summary>
    ///     The NpcParam fields of the body's size (§4.4), in whichever games have them: the map and character capsules
    ///     (SDT's crouching ones, BB's second one), the capsule's offset, the foot IK error limit, the wheel radius and
    ///     the look-at height. True: -1 means "not set" there and is kept.
    /// </summary>
    public static readonly (string Field, bool MinusOneUnset)[] BodyFields =
    {
        ("hitHeight", false), ("hitRadius", false), ("hitYOffset", false), ("hitOffset", false),
        ("chrHitHeight", false), ("chrHitRadius", false),
        ("squatMapHitHeight", true), ("squatMapHitRadius", true), ("squatChrHitHeight", true), ("squatChrHitRadius", true),
        ("subHitOffset", false), ("subHitHeight", false), ("subHitRadius", false),
        ("footIkErrorHeightLimit", false), ("wheelRotRadius", false), ("lookAtHeightOffset", false)
    };

    public static string ChrbndPath(string character) => $"/chr/{character}.chrbnd.dcx";

    public static string BehbndPath(string character) => $"/chr/{character}.behbnd.dcx";

    /// <summary>A behavior character file in a binder: characters/cXXXX.hkx (and DS3's .hkt copy beside it)</summary>
    public static bool IsBehaviorCharacterFile(string name)
    {
        var path = name.Replace('\\', '/').ToLowerInvariant();
        return path.Contains("/characters/") && (path.EndsWith(".hkx") || path.EndsWith(".hkt"));
    }

    /// <summary>
    ///     The NpcParam edits for the body size fields: every row whose model is this character (rows named after it
    ///     that swap in no other model through normalChangeModelId, and rows of other characters that swap this one
    ///     in). The model is scaled in place, so every row using it gets the new size. Values × s; unset ones kept.
    /// </summary>
    /// <param name="rowCharacter">The character a row ID names (LuaNpc.CharacterOf), or null</param>
    public static List<(Andre.Formats.Param.Row Row, IReadOnlyList<(string Field, object Value)> Values)> PlanNpcParamEdits(
        Andre.Formats.Param npcParam, string character, Func<int, string?> rowCharacter, float s)
    {
        var edits = new List<(Andre.Formats.Param.Row, IReadOnlyList<(string, object)>)>();
        if (!int.TryParse(character.AsSpan(1), out var number))
            return edits;

        foreach (var row in npcParam.Rows)
        {
            string? model = rowCharacter(row.ID);
            if (row["normalChangeModelId"] is { } swap && Convert.ToInt32(swap.Value) is > 0 and <= 9999 and var swapped)
                model = $"c{swapped:D4}";

            if (!string.Equals(model, character, StringComparison.OrdinalIgnoreCase))
                continue;

            var values = new List<(string, object)>();
            foreach (var (field, minusOneUnset) in BodyFields)
            {
                if (row[field] is not { Value: float v })
                    continue;
                if (minusOneUnset && v == -1f)
                    continue;
                var scaled = HavokFieldScale.Mul(v, s);
                if (scaled != v)
                    values.Add((field, scaled));
            }

            if (values.Count > 0)
                edits.Add((row, values));
        }

        return edits;
    }

    /// <summary>
    ///     What scaling a character would read and write, and whether it can (an in-place scale of a character that
    ///     shares clips with others is refused: Character Scale Plan.md D2).
    /// </summary>
    /// <param name="sharedWith">Characters that play this character's clips (NpcParam normalChangeAnimChrId)</param>
    public static CharacterScalePlan CreatePlan(VirtualFileSystem fs, ProjectType game, string character,
        CharacterScaleSettings settings, IEnumerable<string>? sharedWith = null)
    {
        var plan = new CharacterScalePlan
        {
            Game = game,
            Character = character.ToLowerInvariant(),
            Settings = settings,
            SharedWith = sharedWith?.Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(n => n).ToList() ?? new()
        };

        plan.NotScaled.Add(LOC.Get("CHRSCALE_Not_Scaled_Params"));

        // The player is assembled from equipment models (parts), which are their own files
        if (plan.Character == "c0000")
            plan.NotScaled.Add(LOC.Get("CHRSCALE_Not_Scaled_Parts"));

        if (!IsSupportedGame(game))
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Game", game.ToString()));

        if (!float.IsFinite(settings.Scale) || settings.Scale <= 0)
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Scale_Invalid"));
        else if (MathF.Abs(settings.Scale - 1f) < 1e-6f)
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Scale_One"));

        var chrbnd = ChrbndPath(plan.Character);
        if (fs.FileExists(chrbnd))
        {
            plan.ModelBinders.Add(chrbnd);
            try
            {
                using var reader = AnimationPackage.OpenBinder(game, fs.ReadFileOrThrow(chrbnd));
                plan.ModelCount = reader.Files.Count(f => IsFlver(f.Name));
                plan.PhysicsFileCount = reader.Files.Count(f => f.ID / 100 == 3 &&
                                                               f.Name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase));
                plan.ClothFileCount = reader.Files.Count(f => f.Name.EndsWith("_c.hkx", StringComparison.OrdinalIgnoreCase) ||
                                                             f.Name.EndsWith("_cloth.hkx", StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception e)
            {
                plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Read", chrbnd, e.Message));
            }
        }
        else
        {
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_No_Chrbnd", chrbnd));
        }

        var hasSkeleton = false;
        foreach (var path in AnimationPackage.FindCharacterBinders(fs, chrbnd, plan.Character))
        {
            // The file system can answer with backslashes; written and listed with forward ones
            plan.AnimationBinders.Add(path.Replace('\\', '/'));
            try
            {
                using var reader = AnimationPackage.OpenBinder(game, fs.ReadFileOrThrow(path));
                var ids = reader.Files.Select(f => f.ID).ToList();
                if (plan.AnimationBinders.Count == 1 && ids.Contains(AnimationPackage.SkeletonBinderId(ids)))
                    hasSkeleton = true;
                plan.ClipCount += reader.Files.Count(f => AnimationPackage.IsAnimationFile(game, f.ID, f.Name));
            }
            catch (Exception e)
            {
                plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Read", path, e.Message));
            }
        }

        // Behavior character files: in the behbnd, and in BB in the anibnd
        var behbnd = BehbndPath(plan.Character);
        foreach (var path in plan.AnimationBinders.Take(1).Prepend(fs.FileExists(behbnd) ? behbnd : null))
        {
            if (path == null)
                continue;
            try
            {
                using var reader = AnimationPackage.OpenBinder(game, fs.ReadFileOrThrow(path));
                var count = reader.Files.Count(f => IsBehaviorCharacterFile(f.Name));
                if (count > 0)
                {
                    plan.BehaviorBinders.Add(path);
                    plan.BehaviorFileCount += count;
                }
            }
            catch (Exception e)
            {
                plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Read", path, e.Message));
            }
        }

        // Time acts: in the anibnds, and in AC6 the behbnd (tae_action)
        foreach (var path in plan.AnimationBinders.Append(fs.FileExists(behbnd) ? behbnd : null))
        {
            if (path == null)
                continue;
            try
            {
                using var reader = AnimationPackage.OpenBinder(game, fs.ReadFileOrThrow(path));
                var count = reader.Files.Count(f => f.Name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase));
                if (count > 0)
                {
                    plan.TimeActBinders.Add(path);
                    plan.TimeActFileCount += count;
                }
            }
            catch (Exception e)
            {
                plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Read", path, e.Message));
            }
        }

        // Own skeleton, no clips: it plays another character's, which an in-place scale would resize too (§4.1)
        if (hasSkeleton && plan.ClipCount == 0)
        {
            plan.BorrowsClips = true;
            if (settings.Clips)
                plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Borrows_Clips"));
        }

        if (plan.SharedWith.Count > 0 && (settings.Clips || settings.TimeActs))
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Shared", string.Join(", ", plan.SharedWith)));

        if (!settings.Models && !settings.Ragdoll && !settings.Cloth && !settings.Skeleton && !settings.Clips && !settings.Behavior && !settings.BodyParams && !settings.TimeActs && !settings.LinkedParams)
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_Nothing"));

        return plan;
    }

    /// <summary>
    ///     Writes the scaled files. Each binder is read from <paramref name="fs" /> (the project's files over the
    ///     game's) and handed to <paramref name="write" /> whole; a binder that fails is not written.
    /// </summary>
    public static CharacterScaleResult Apply(CharacterScalePlan plan, VirtualFileSystem fs, Action<string, byte[]> write,
        IProgress<(int Done, int Total)>? progress = null, CancellationToken cancel = default)
    {
        var result = new CharacterScaleResult();
        if (!plan.CanApply)
        {
            result.Failures.AddRange(plan.Refusals);
            return result;
        }

        var s = plan.Settings.Scale;
        var total = (plan.Settings.Models ? plan.ModelCount : 0) + (plan.Settings.Clips ? plan.ClipCount : 0) + 1;
        var done = 0;
        void Step(int n = 1) => progress?.Report((Interlocked.Add(ref done, n), total));

        if (plan.Settings.Models || plan.Settings.Ragdoll || plan.Settings.Cloth)
        {
            var physicsRules = PhysicsRules(plan.Settings.ScaleMass);
            var profile = HavokAnimationReader.ProfileOf(plan.Game);
            foreach (var path in plan.ModelBinders)
            {
                cancel.ThrowIfCancellationRequested();
                try
                {
                    var bnd = ReadBinder(plan.Game, fs.ReadFileOrThrow(path));
                    var changed = false;
                    if (plan.Settings.Models)
                    {
                        foreach (var file in bnd.Files.Where(f => IsFlver(f.Name)))
                        {
                            var (raw, dcx) = Unpack(file.Bytes);
                            var flver = FLVER2.Read(raw);
                            result.Vertices += ScaleFlver(flver, s);
                            file.Bytes = Pack(flver.Write(), dcx);
                            result.Models++;
                            changed = true;
                            Step();
                        }
                    }

                    if (plan.Settings.Ragdoll)
                    {
                        foreach (var file in bnd.Files.Where(IsPhysicsFile))
                        {
                            var (raw, dcx) = Unpack(file.Bytes);
                            var document = Havok.Files.HavokFile.Read(raw, profile);
                            var values = HavokFieldScale.Apply(document, physicsRules, s);
                            if (values == 0)
                                continue;
                            file.Bytes = Pack(Havok.Files.HavokFile.Write(document), dcx);
                            result.PhysicsFiles++;
                            result.PhysicsValues += values;
                            changed = true;
                        }
                    }

                    if (plan.Settings.Cloth)
                    {
                        foreach (var file in bnd.Files.Where(IsClothFile))
                        {
                            var (raw, dcx) = Unpack(file.Bytes);
                            var document = Havok.Files.HavokFile.Read(raw, profile);
                            var values = HavokFieldScale.Apply(document, ClothRules, s) + ClothFrame.ScaleOperators(document, s);
                            if (values == 0)
                                continue;
                            file.Bytes = Pack(Havok.Files.HavokFile.Write(document), dcx);
                            result.ClothFiles++;
                            result.ClothValues += values;
                            changed = true;
                        }
                    }

                    if (!changed)
                        continue;
                    write(path, WriteBinder(bnd));
                    result.Written.Add(path);
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    result.Failures.Add($"{path}: {e.Message}");
                }
            }
        }

        // Behbnds (behavior files; AC6's time acts); one in an anibnd (BB) is scaled with the anibnd, written once
        var behbnds = (plan.Settings.Behavior ? plan.BehaviorBinders : new List<string>())
            .Concat(plan.Settings.TimeActs ? plan.TimeActBinders : new List<string>())
            .Where(p => !plan.AnimationBinders.Contains(p)).Distinct().ToList();
        foreach (var path in behbnds)
        {
            cancel.ThrowIfCancellationRequested();
            try
            {
                var bnd = ReadBinder(plan.Game, fs.ReadFileOrThrow(path));
                var changed = false;
                if (plan.Settings.Behavior && plan.BehaviorBinders.Contains(path))
                    changed |= ScaleBehaviorFiles(plan, bnd, path, result);
                if (plan.Settings.TimeActs && plan.TimeActBinders.Contains(path))
                    changed |= ScaleTimeActFiles(plan, bnd, result) > 0;
                if (changed)
                {
                    write(path, WriteBinder(bnd));
                    result.Written.Add(path);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                result.Failures.Add($"{path}: {e.Message}");
            }
        }

        var inAnibnd = plan.Settings.Behavior && plan.BehaviorBinders.Any(plan.AnimationBinders.Contains) ||
                       plan.Settings.TimeActs && plan.TimeActBinders.Any(plan.AnimationBinders.Contains);
        if (plan.Settings.Skeleton || plan.Settings.Clips || inAnibnd)
            ScaleAnimations(plan, fs, write, result, Step, cancel);

        progress?.Report((total, total));
        return result;
    }

    /// <summary>The behavior character files of a binder × s by <see cref="BehaviorRules" />. True if any changed.</summary>
    private static bool ScaleBehaviorFiles(CharacterScalePlan plan, IBinder bnd, string binderPath, CharacterScaleResult result)
    {
        var profile = HavokAnimationReader.ProfileOf(plan.Game);
        HavokCompendium? compendium = null;
        if (bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".compendium", StringComparison.OrdinalIgnoreCase)) is { } c)
            compendium = HavokAnimationReader.ReadCompendium(plan.Game, c.Bytes.ToArray());

        var changed = false;
        foreach (var file in bnd.Files.Where(f => IsBehaviorCharacterFile(f.Name)))
        {
            var name = Path.GetFileName(file.Name.Replace('\\', '/'));
            HavokDocument document;
            DCX.Type dcx;
            try
            {
                (var raw, dcx) = Unpack(file.Bytes);
                document = Havok.Files.HavokFile.Read(raw, profile, compendium);
            }
            catch (Exception e)
            {
                // DS3's .hkt copies may be in a form the reader doesn't take: left, and named in the result
                result.BehaviorSkipped.Add($"{binderPath}: {name} ({e.Message.Split('\n')[0]})");
                continue;
            }

            var values = HavokFieldScale.Apply(document, BehaviorRules, plan.Settings.Scale);
            if (values == 0)
                continue;

            file.Bytes = Pack(Havok.Files.HavokFile.Write(document), dcx);
            result.BehaviorFiles++;
            result.BehaviorValues += values;
            changed = true;
        }

        return changed;
    }

    private static void ScaleAnimations(CharacterScalePlan plan, VirtualFileSystem fs, Action<string, byte[]> write,
        CharacterScaleResult result, Action<int> step, CancellationToken cancel)
    {
        var s = plan.Settings.Scale;
        var profile = HavokAnimationReader.ProfileOf(plan.Game);

        // Clips are decoded against the skeleton as the game has it (unscaled): its bones and reference floats bind
        // the tracks; the translations come from the clip itself
        HavokSkeletonData? skeleton = null;

        // The tolerance in world units grows with the character, so the error stays the same against its size
        var encode = SplineEncodeSettings.ForCharacterClips();
        encode.PositionTolerance *= s;

        foreach (var path in plan.AnimationBinders)
        {
            cancel.ThrowIfCancellationRequested();
            try
            {
                var bnd = ReadBinder(plan.Game, fs.ReadFileOrThrow(path));
                var ids = bnd.Files.Select(f => f.ID).ToList();

                HavokCompendium? compendium = null;
                if (bnd.Files.FirstOrDefault(f => f.ID == AnimationPackage.CompendiumBinderId) is { } compendiumFile)
                    compendium = HavokAnimationReader.ReadCompendium(plan.Game, compendiumFile.Bytes.ToArray());

                var changed = false;
                var skeletonId = AnimationPackage.SkeletonBinderId(ids);
                if (bnd.Files.FirstOrDefault(f => f.ID == skeletonId) is { } skeletonFile)
                {
                    skeleton ??= HavokAnimationReader.ReadSkeleton(plan.Game, skeletonFile.Bytes.ToArray(), compendium);
                    if (plan.Settings.Skeleton)
                    {
                        var (raw, dcx) = Unpack(skeletonFile.Bytes);
                        var document = Havok.Files.HavokFile.Read(raw, profile, compendium);
                        result.SkeletonBones += HavokFieldScale.Apply(document, SkeletonRules, s);
                        skeletonFile.Bytes = Pack(Havok.Files.HavokFile.Write(document), dcx);
                        changed = true;
                    }
                }

                if (plan.Settings.Behavior && plan.BehaviorBinders.Contains(path))
                    changed |= ScaleBehaviorFiles(plan, bnd, path, result);

                if (plan.Settings.TimeActs && plan.TimeActBinders.Contains(path))
                    changed |= ScaleTimeActFiles(plan, bnd, result) > 0;

                if (plan.Settings.Clips)
                {
                    var clips = bnd.Files.Where(f => AnimationPackage.IsAnimationFile(plan.Game, f.ID, f.Name)).ToList();
                    if (clips.Count > 0 && skeleton == null)
                    {
                        result.Failures.Add(LOC.Get("CHRSCALE_Fail_No_Skeleton", path));
                    }
                    else if (clips.Count > 0)
                    {
                        var failures = new ConcurrentBag<string>();
                        var worst = new ConcurrentBag<(double Ratio, string Name)>();
                        var options = new ParallelOptions
                        {
                            CancellationToken = cancel,
                            MaxDegreeOfParallelism = plan.Settings.Parallel ? Math.Max(1, Environment.ProcessorCount - 1) : 1
                        };
                        var written = 0;
                        var exact = 0;
                        System.Threading.Tasks.Parallel.ForEach(clips, options, file =>
                        {
                            var name = Path.GetFileName(file.Name.Replace('\\', '/'));
                            try
                            {
                                var (raw, dcx) = Unpack(file.Bytes);

                                // Exact first: spline ranges, uncompressed translations and root motion × s
                                var document = Havok.Files.HavokFile.Read(raw, profile, compendium);
                                if (TryScaleClipExact(document, s))
                                {
                                    file.Bytes = Pack(Havok.Files.HavokFile.Write(document), dcx);
                                    Interlocked.Increment(ref exact);
                                    Interlocked.Increment(ref written);
                                    return;
                                }

                                // Else decoded, scaled and encoded again (within the encoder's tolerance)
                                var clip = EditableClip.Read(plan.Game, raw, compendium, skeleton!);
                                if (clip == null)
                                {
                                    failures.Add(LOC.Get("CHRSCALE_Fail_Clip_Decode", name));
                                    return;
                                }

                                var bytes = ScaleClip(clip, s).Write(plan.Game, raw, compendium, out var clipResult,
                                    HavokClipFormat.Auto, encode);
                                file.Bytes = Pack(bytes, dcx);
                                worst.Add((clipResult.WorstErrorRatio, name));
                                Interlocked.Increment(ref written);
                            }
                            catch (Exception e) when (e is not OperationCanceledException)
                            {
                                failures.Add($"{name}: {e.Message}");
                            }
                            finally
                            {
                                step(1);
                            }
                        });

                        result.Clips += written;
                        result.ClipsExact += exact;
                        result.ClipsFailed += failures.Count;
                        result.Failures.AddRange(failures.OrderBy(f => f));
                        if (!worst.IsEmpty)
                        {
                            var w = worst.MaxBy(x => x.Ratio);
                            if (w.Ratio > result.WorstErrorRatio)
                            {
                                result.WorstErrorRatio = w.Ratio;
                                result.WorstClip = w.Name;
                            }
                        }

                        changed |= written > 0;
                    }
                }

                if (changed)
                {
                    write(path, WriteBinder(bnd));
                    result.Written.Add(path);
                }
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                result.Failures.Add($"{path}: {e.Message}");
            }
        }
    }

    /// <summary>
    ///     A FLVER's lengths × s: vertex positions, bone translations and bounding boxes, dummy positions, mesh and
    ///     header bounding boxes. Normals, tangents, rotations, bone scales and mesh box rotations stay. Returns the
    ///     vertices changed.
    /// </summary>
    public static int ScaleFlver(FLVER2 flver, float s)
    {
        flver.Header.BoundingBoxMin = Mul(flver.Header.BoundingBoxMin, s);
        flver.Header.BoundingBoxMax = Mul(flver.Header.BoundingBoxMax, s);

        foreach (var node in flver.Nodes)
        {
            node.Translation = Mul(node.Translation, s);
            node.BoundingBoxMin = Mul(node.BoundingBoxMin, s);
            node.BoundingBoxMax = Mul(node.BoundingBoxMax, s);
        }

        foreach (var dummy in flver.Dummies)
            dummy.Position = Mul(dummy.Position, s);

        // Mesh boxes: up to DS3 (version < 0x2001A) Min / Max are the vertices' bounds. From SDT on (with the third
        // vector) the box is oriented: Min holds the half-extents, Max its rotation (angles: 2π and π/1000 occur) and
        // Unk its centre, checked against the vertices on ER and SDT (Character Scale Plan.md §4.3)
        var orientedBoxes = flver.Header.Version >= 0x2001A;
        var vertices = 0;
        foreach (var mesh in flver.Meshes)
        {
            if (mesh.BoundingBox is { } box)
            {
                box.Min = Mul(box.Min, s);
                if (orientedBoxes)
                    box.Unk = Mul(box.Unk, s);
                else
                    box.Max = Mul(box.Max, s);
            }

            foreach (var vertex in mesh.Vertices)
            {
                vertex.Position = Mul(vertex.Position, s);
                vertices++;
            }
        }

        return vertices;
    }

    /// <summary>The clip fields scaled as stored: root motion positions, uncompressed translations (§4.1)</summary>
    public static readonly HavokScaleRule[] ClipRules =
    {
        new("hkaDefaultAnimatedReferenceFrame", "m_referenceFrameSamples[]", HavokScaleOp.Xyz),
        new("hkaInterleavedUncompressedAnimation", "m_transforms[].m_translation", HavokScaleOp.Xyz)
    };

    /// <summary>
    ///     Scales a clip file's animation in place, without encoding it again. A spline clip stores each translation
    ///     channel as a static float or as quantized control points between a block's min and max, so those floats
    ///     × s scale every decoded translation exactly while rotations keep their bytes and the data its layout.
    ///     False (document unchanged) when the file holds an animation of another kind, which is then encoded again.
    /// </summary>
    public static bool TryScaleClipExact(HavokDocument document, float s)
    {
        var animations = document.Objects.Where(o => o.GetType().Name is "hkaSplineCompressedAnimation"
            or "hkaInterleavedUncompressedAnimation").ToList();
        var others = document.ObjectsOf<Havok.Shared.IHavokAnimation>().Count(a => !animations.Contains(a));
        if (animations.Count == 0 || others > 0)
            return false;

        // Lay every spline out first, so nothing is changed unless all of them can be
        var splines = new List<(object Animation, List<SplineCompressedLayout.Block> Blocks, Havok.Shared.IHavokSplineAnimation View)>();
        foreach (var animation in animations)
        {
            if (animation is not Havok.Shared.IHavokSplineAnimation spline)
            {
                // A spline this SDK has no view of can't be laid out: encoded again instead
                if (animation.GetType().Name == "hkaSplineCompressedAnimation")
                    return false;
                continue;
            }

            var blocks = SplineCompressedLayout.Read(spline.Data.ToArray(), spline.TransformTrackCount,
                spline.FloatTrackCount, spline.BlockOffsets, spline.FloatBlockOffsets);
            splines.Add((animation, blocks, spline));
        }

        // The layout must give back the file's own bytes before anything is scaled. Some vanilla clips end with 16
        // bytes of padding past their last block, which the writer doesn't emit: those are kept from the file.
        var rewritten = new List<(object Animation, byte[] Data)>();
        foreach (var (animation, blocks, view) in splines)
        {
            var original = view.Data.ToArray();
            var plain = SplineCompressedLayout.Write(blocks);
            if (!FitsLayout(plain, original, view))
                return false;

            foreach (var block in blocks)
                foreach (var track in block.Transforms)
                {
                    var p = track.Position;
                    for (var c = 0; c < 3; c++)
                    {
                        p.Static[c] = HavokFieldScale.Mul(p.Static[c], s);
                        p.Min[c] = HavokFieldScale.Mul(p.Min[c], s);
                        p.Max[c] = HavokFieldScale.Mul(p.Max[c], s);
                    }
                }

            // Only floats changed, so the scaled blocks take exactly the bytes the plain ones did
            var scaled = SplineCompressedLayout.Write(blocks);
            if (scaled.Data.Length != plain.Data.Length || !scaled.BlockOffsets.SequenceEqual(plain.BlockOffsets))
                return false;

            var data = (byte[])original.Clone();
            scaled.Data.CopyTo(data, 0);
            rewritten.Add((animation, data));
        }

        foreach (var (animation, data) in rewritten)
            SetBytes(animation, "m_data", data);

        HavokFieldScale.Apply(document, ClipRules, s);
        return true;
    }

    // The writer's bytes are the file's (up to the padding some files carry after their last block), at the same offsets
    private static bool FitsLayout(SplineCompressedLayout.Written written, byte[] original,
        Havok.Shared.IHavokSplineAnimation view)
    {
        if (written.Data.Length > original.Length || original.Length - written.Data.Length > 16)
            return false;
        if (!written.BlockOffsets.SequenceEqual(view.BlockOffsets))
            return false;
        if (view.FloatTrackCount > 0 && !written.FloatBlockOffsets.SequenceEqual(view.FloatBlockOffsets))
            return false;

        return written.Data.AsSpan().SequenceEqual(original.AsSpan(0, written.Data.Length));
    }

    // Sets a byte list or array field (the SDKs differ) to new bytes of the same length
    private static void SetBytes(object owner, string fieldName, byte[] bytes)
    {
        var field = owner.GetType().GetField(fieldName, System.Reflection.BindingFlags.Public |
                                                        System.Reflection.BindingFlags.Instance)
                    ?? throw new MissingFieldException(owner.GetType().Name, fieldName);
        switch (field.GetValue(owner))
        {
            case List<byte> list:
                list.Clear();
                list.AddRange(bytes);
                break;
            case byte[] array when array.Length == bytes.Length:
                bytes.CopyTo(array, 0);
                break;
            default:
                field.SetValue(owner, bytes);
                break;
        }
    }

    /// <summary>A clip with every track's translation and the root motion's position × s (turns, rotations, scales kept)</summary>
    public static EditableClip ScaleClip(EditableClip clip, float s)
    {
        var frames = new HavokTransformData[clip.FrameCount][];
        for (var f = 0; f < clip.FrameCount; f++)
        {
            var pose = clip.Frames[f];
            var scaled = new HavokTransformData[pose.Length];
            for (var t = 0; t < pose.Length; t++)
            {
                scaled[t] = pose[t];
                scaled[t].Translation = Mul(pose[t].Translation, s);
            }

            frames[f] = scaled;
        }

        var root = clip.RootMotion?.Select(r => new Vector4(HavokFieldScale.Mul(r.X, s), HavokFieldScale.Mul(r.Y, s),
            HavokFieldScale.Mul(r.Z, s), r.W)).ToArray();
        return clip.WithFrames(frames, clip.Floats, root);
    }

    private static Vector3 Mul(Vector3 v, float s) =>
        new(HavokFieldScale.Mul(v.X, s), HavokFieldScale.Mul(v.Y, s), HavokFieldScale.Mul(v.Z, s));

    private static bool IsFlver(string name) =>
        Path.GetFileName(name.Replace('\\', '/')).Contains(".flver", StringComparison.OrdinalIgnoreCase);

    private static (byte[] Raw, DCX.Type Dcx) Unpack(Memory<byte> data)
    {
        if (!DCX.Is(data))
            return (data.ToArray(), DCX.Type.None);

        var raw = DCX.Decompress(data.ToArray(), out var type);
        return (raw.ToArray(), type);
    }

    private static byte[] Pack(byte[] raw, DCX.Type dcx) => dcx == DCX.Type.None ? raw : DCX.Compress(raw, dcx).ToArray();

    private static IBinder ReadBinder(ProjectType game, Memory<byte> data) =>
        AnimationPackage.UsesLegacyBinders(game) ? BND3.Read(data) : BND4.Read(data);

    private static byte[] WriteBinder(IBinder binder) => binder switch
    {
        BND3 bnd3 => bnd3.Write(),
        BND4 bnd4 => bnd4.Write(),
        _ => throw new NotSupportedException(binder.GetType().Name)
    };
}
