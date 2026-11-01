#nullable enable
using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.Common;

/// <summary>
///     A part model of a character.
/// </summary>
/// <param name="Name">The name of the model.</param>
/// <param name="AttachNode">The bone that holds the model, for parts that aren't skinned to the character.</param>
/// <param name="EquipParamId">The EquipParamWeapon row of a weapon, or -1 if there is none.</param>
public sealed record CharacterPart(string Name, string? AttachNode = null, int EquipParamId = -1);

/// <summary>
///     The part models a character is built from.
/// </summary>
/// <param name="Parts">The part models.</param>
/// <param name="Masks">The enabled material masks of the skinned parts. Null if everything is enabled.</param>
public sealed record CharacterAppearance(IReadOnlyList<CharacterPart> Parts, int[]? Masks);

/// <summary>
///     The places on a character that hold a part model.
/// </summary>
public enum CharacterSlot
{
    Head,
    Body,
    Arms,
    Legs,
    RightHand,
    LeftHand
}

/// <summary>
///     A set of part models chosen by hand, rather than coming from CharaInitParam.
/// </summary>
public sealed class CharacterLoadout
{
    public bool IsFemale { get; set; }

    /// <summary>
    ///     The part model in each slot. Slots that aren't listed are empty.
    /// </summary>
    public Dictionary<CharacterSlot, string> Parts { get; } = new();
}

/// <summary>
///     Builds player-like characters. The FLVER of c0000 only holds a skeleton: what is drawn is the
///     body from the parts folder, and the armor and weapons on top of it come from CharaInitParam.
///     Faces (fg_*) are not displayed yet.
/// </summary>
public static class CharacterComposition
{
    public const string CompositeModel = "c0000";

    private const string BodyPartPrefix = "fc_";

    // Larger than the invisible flags of any game, as masks past the end of the list are treated as disabled
    private const int MaskCount = 128;

    private const int SexMale = 1;

    private const int ModelGenderUnisex = 0;
    private const int ModelGenderMale = 1;
    private const int ModelGenderFemale = 2;
    private const int ModelGenderPerSex = 3;

    private const string RightHandNode = "R_Weapon";
    private const string LeftHandNode = "L_Weapon";

    // Rolling around the forearm turns a blade around, and puts the plate of a shield on the outside
    private static readonly Matrix4x4 WeaponRoll = Matrix4x4.CreateRotationX(MathF.PI);

    // Turning around the length of a weapon, for one sitting on a dummy poly
    private static readonly Matrix4x4 WeaponTurn = Matrix4x4.CreateRotationY(MathF.PI);

    // Elden Ring leaves the plain flags empty, and uses the SexVer ones instead
    private static readonly Regex InvisibleFlag = new(@"^invisibleFlag(?:_SexVer)?(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly ConditionalWeakTable<ProjectEntry, HashSet<string>> _partNames = new();
    private static readonly ConditionalWeakTable<ProjectEntry, ModelRig> _skeletons = new();
    private static readonly ConditionalWeakTable<ProjectEntry, List<FLVER.Dummy>> _bodyDummies = new();

    public static bool IsComposite(string? modelName)
    {
        return CompositeModel.Equals(modelName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Whether a property of a map part decides what the character is built from.
    /// </summary>
    public static bool IsAppearanceProperty(string propertyName)
    {
        return propertyName is "CharaInitID" or "CharaInitParamId";
    }

    /// <summary>
    ///     Gets the parts of a character.
    /// </summary>
    /// <param name="charaInitId">The CharaInitParam row to dress it from. Anything else gives the bare body.</param>
    public static CharacterAppearance Resolve(ProjectEntry project, int charaInitId = -1)
    {
        HashSet<string> partNames = GetPartNames(project);

        Param.Row? charaInit = GetRow(project, "CharaInitParam", charaInitId);

        var isMale = charaInit == null || GetValue(charaInit, "npcPlayerSex") == SexMale;
        var sex = isMale ? "m" : "f";

        List<CharacterPart> parts = new();

        void AddFirstFound(params string[] names)
        {
            if (names.FirstOrDefault(partNames.Contains) is { } name)
            {
                parts.Add(new CharacterPart(name));
            }
        }

        // Dark Souls keeps the bare body in the equipment slots, where the armor replaces it. Its head is empty.
        // The later games have one body under all of the armor, stored as a face model.
        var hasSplitBody = project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R;

        void AddBareSlot(string prefix)
        {
            if (hasSplitBody && prefix != "hd")
            {
                AddFirstFound($"{prefix}_{sex}_0000", $"{prefix}_m_0000");
            }
        }

        AddFirstFound(GetBodyPartNames(sex));

        if (charaInit == null)
        {
            AddBareSlot("bd");
            AddBareSlot("am");
            AddBareSlot("lg");

            return new CharacterAppearance(parts, null);
        }

        var masks = new int[MaskCount];
        Array.Fill(masks, 1);

        void AddEquipment(string prefix, params string[] fields)
        {
            var field = fields.FirstOrDefault(f => HasField(charaInit, f));
            if (field == null || FindEquipment(project, "EquipParamProtector", GetValue(charaInit, field)) is not { } protector)
            {
                AddBareSlot(prefix);
                return;
            }

            HideMasks(protector, masks);

            var id = GetValue(protector, "equipModelId");
            if (id <= 0)
            {
                AddBareSlot(prefix);
                return;
            }

            var names = GetModelGenderLetters(GetValue(protector, "equipModelGender"), sex)
                .Select(letter => $"{prefix}_{letter}_{id:D4}")
                .ToArray();

            AddFirstFound(names);
        }

        AddEquipment("hd", "equip_Helm");
        AddEquipment("bd", "equip_Armer", "equip_Armor");
        AddEquipment("am", "equip_Gaunt");
        AddEquipment("lg", "equip_Leg");

        void AddWeapon(string field, string node)
        {
            if (FindEquipment(project, "EquipParamWeapon", GetValue(charaInit, field)) is not { } weapon)
            {
                return;
            }

            var id = GetValue(weapon, "equipModelId");
            var name = $"wp_a_{id:D4}";

            if (id > 0 && partNames.Contains(name))
            {
                parts.Add(new CharacterPart(name, node, weapon.ID));
            }
        }

        AddWeapon("equip_Wep_Right", RightHandNode);
        AddWeapon("equip_Wep_Left", LeftHandNode);

        return new CharacterAppearance(parts, masks);
    }

    /// <summary>
    ///     Gets the parts of a character dressed by hand.
    /// </summary>
    public static CharacterAppearance Resolve(ProjectEntry project, CharacterLoadout loadout)
    {
        HashSet<string> partNames = GetPartNames(project);

        var sex = loadout.IsFemale ? "f" : "m";
        var hasSplitBody = project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R;

        List<CharacterPart> parts = new();

        if (GetBodyPartNames(sex).FirstOrDefault(partNames.Contains) is { } body)
        {
            parts.Add(new CharacterPart(body));
        }

        var masks = new int[MaskCount];
        Array.Fill(masks, 1);

        foreach (CharacterSlot slot in Enum.GetValues<CharacterSlot>())
        {
            var isWeapon = slot is CharacterSlot.RightHand or CharacterSlot.LeftHand;

            if (loadout.Parts.TryGetValue(slot, out var name) && partNames.Contains(name))
            {
                if (isWeapon)
                {
                    parts.Add(new CharacterPart(name, slot is CharacterSlot.RightHand ? RightHandNode : LeftHandNode,
                        FindWeaponRow(project, name)));
                }
                else
                {
                    parts.Add(new CharacterPart(name));

                    if (FindProtector(project, slot, name) is { } protector)
                    {
                        HideMasks(protector, masks);
                    }
                }
            }
            else if (hasSplitBody && !isWeapon && slot is not CharacterSlot.Head)
            {
                var prefix = GetSlotPrefix(slot);

                if (new[] { $"{prefix}_{sex}_0000", $"{prefix}_m_0000" }.FirstOrDefault(partNames.Contains) is { } bare)
                {
                    parts.Add(new CharacterPart(bare));
                }
            }
        }

        return new CharacterAppearance(parts, masks);
    }

    /// <summary>
    ///     Gets the names of the part models that can fill a slot.
    /// </summary>
    public static IReadOnlyList<string> GetSlotParts(ProjectEntry project, CharacterSlot slot)
    {
        Regex name = new($@"^{GetSlotPrefix(slot)}_[a-z]_\d{{4}}$");

        return GetPartNames(project).Where(p => name.IsMatch(p)).OrderBy(p => p, StringComparer.Ordinal).ToList();
    }

    private static string GetSlotPrefix(CharacterSlot slot)
    {
        return slot switch
        {
            CharacterSlot.Head => "hd",
            CharacterSlot.Body => "bd",
            CharacterSlot.Arms => "am",
            CharacterSlot.Legs => "lg",
            _ => "wp"
        };
    }

    // The body is stored as a face model, and these are in order of preference
    private static string[] GetBodyPartNames(string sex)
    {
        return new[] { $"fc_{sex}_0100", $"fc_{sex}_0000", "fc_m_0100", "fc_m_0000" };
    }

    /// <summary>
    ///     Whether a part is held in the left hand.
    /// </summary>
    public static bool IsLeftHand(CharacterPart part)
    {
        return part.AttachNode == LeftHandNode;
    }

    // A chosen model comes without a row, so use the first weapon that uses the model
    private static int FindWeaponRow(ProjectEntry project, string partName)
    {
        if (project.Handler.ParamData?.PrimaryBank?.Params is not { } bank ||
            !bank.TryGetValue("EquipParamWeapon", out Param? param) ||
            !int.TryParse(partName.AsSpan(partName.Length - 4), out var modelId))
        {
            return -1;
        }

        return param.Rows.FirstOrDefault(r => GetValue(r, "equipModelId") == modelId)?.ID ?? -1;
    }

    // A chosen model comes without a row, so use a row that wears it to find out what it covers
    private static Param.Row? FindProtector(ProjectEntry project, CharacterSlot slot, string partName)
    {
        if (project.Handler.ParamData?.PrimaryBank?.Params is not { } bank ||
            !bank.TryGetValue("EquipParamProtector", out Param? param) ||
            !int.TryParse(partName.AsSpan(partName.Length - 4), out var modelId))
        {
            return null;
        }

        var slotField = slot switch
        {
            CharacterSlot.Head => "headEquip",
            CharacterSlot.Body => "bodyEquip",
            CharacterSlot.Arms => "armEquip",
            _ => "legEquip"
        };

        return param.Rows.FirstOrDefault(r => GetValue(r, "equipModelId") == modelId && GetValue(r, slotField) != 0);
    }

    /// <summary>
    ///     Creates a mesh of all the parts of a character, and loads them. Returns null if it has no parts.
    ///     The submeshes of the mesh are the parts, in order.
    /// </summary>
    public static MeshRenderableProxy? CreateMesh(ProjectEntry project, RenderScene scene, CharacterAppearance appearance,
        string uid, string jobName)
    {
        if (appearance.Parts.Count == 0)
        {
            return null;
        }

        List<MeshProvider> providers = new();

        foreach (CharacterPart part in appearance.Parts)
        {
            ResourceDescriptor asset = ModelLocator.GetPartsModel(project, part.Name, part.Name);

            // The masks are for the meshes of the body and armor, and mean something else on a weapon
            FlverMeshProvider provider = MeshProviderCache.GetFlverMeshProvider(
                ResourceManager.DefaultPool.Find<FlverResource>(asset.Name),
                part.AttachNode == null ? appearance.Masks : null, uid);

            // The fur on armor is left alone, as only the body models have meshes marked as body hair
            var isBody = part.Name.StartsWith(BodyPartPrefix, StringComparison.OrdinalIgnoreCase);

            provider.HideBodyHair = isBody;
            provider.HideHead = isBody && project.Descriptor.ProjectType is ProjectType.ER;

            providers.Add(provider);

            ResourceManager.DefaultPool.LoadAsset<FlverResource>(project, asset, new LoadOptions
            {
                JobName = jobName,
                Types = ResourceType.Flver | ResourceType.Texture,
                AccessLevel = AccessLevel.AccessGPUOptimizedOnly
            });
        }

        MeshRenderableProxy mesh = MeshRenderableProxy.FromProvider(scene, new CompositeMeshProvider(providers));

        // Attached parts have a rest pose of their own: in the hand of the T-pose
        ModelRig skeleton = GetSkeleton(project);

        for (var i = 0; i < appearance.Parts.Count && i < mesh.Submeshes.Count; i++)
        {
            if (appearance.Parts[i].AttachNode != null &&
                GetWeaponAttachment(project, appearance.Parts[i], skeleton) is var (attachNode, offset) &&
                skeleton.FindNode(attachNode) is >= 0 and var node)
            {
                mesh.Submeshes[i].AttachTransform = offset * skeleton.NodeReferenceWorld[node];
            }
        }

        return mesh;
    }

    /// <summary>
    ///     Describes the parts of a mesh made by <see cref="CreateMesh" /> to the animation preview.
    /// </summary>
    public static IReadOnlyList<AnimatedPart> GetAnimatedParts(ProjectEntry project, CharacterAppearance appearance,
        MeshRenderableProxy mesh)
    {
        List<AnimatedPart> parts = new();

        ModelRig skeleton = GetSkeleton(project);

        for (var i = 0; i < appearance.Parts.Count && i < mesh.Submeshes.Count; i++)
        {
            CharacterPart part = appearance.Parts[i];
            ResourceDescriptor asset = ModelLocator.GetPartsModel(project, part.Name, part.Name);

            var attachment = part.AttachNode != null ? GetWeaponAttachment(project, part, skeleton) : ((string, Matrix4x4)?)null;

            parts.Add(new AnimatedPart(part.Name, PathBuilder.GetRelativePath(project, asset.AssetVirtualPath),
                mesh.Submeshes[i], attachment?.Item1, attachment?.Item2));
        }

        return parts;
    }

    /// <summary>
    ///     Gets the textures that bodies and faces share, which are in a container of their own in most games.
    /// </summary>
    public static ResourceDescriptor? GetCommonTexture(ProjectEntry project)
    {
        ResourceDescriptor asset = TextureLocator.GetCharacterCommonTextureVirtualPath(project, "common_body");

        var path = asset.GetContainerPath(project);

        return path != null && project.VFS.FS.FileExists(path) ? asset : null;
    }

    private static HashSet<string> GetPartNames(ProjectEntry project)
    {
        return _partNames.GetValue(project, p => p.Locator.PartFiles.Entries
            .Select(e => e.Filename.ToLower())
            .ToHashSet());
    }

    // The bones weapons attach to are only linked to the hands in the skeleton of the character itself
    private static ModelRig GetSkeleton(ProjectEntry project)
    {
        return _skeletons.GetValue(project, p =>
        {
            ResourceDescriptor asset = ModelLocator.GetChrModel(p, CompositeModel, CompositeModel);
            var path = asset.AssetVirtualPath != null ? PathBuilder.GetRelativePath(p, asset.AssetVirtualPath) : null;

            try
            {
                if (path != null && p.VFS.FS.FileExists(path) && p.VFS.FS.ReadFile(path) is { } container &&
                    ModelRig.Read(p.Descriptor.ProjectType, container, CompositeModel) is { } rig)
                {
                    return rig;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(CharacterComposition), $"Failed to read the skeleton of {CompositeModel}", e);
            }

            return new ModelRig();
        });
    }

    /// <summary>
    ///     Works out how a weapon is held: the bone that carries it, and how it sits on that bone.
    /// </summary>
    private static (string Node, Matrix4x4 Offset) GetWeaponAttachment(ProjectEntry project, CharacterPart part, ModelRig skeleton)
    {
        return GetAbsorptionAttachment(project, part, skeleton)
            ?? (part.AttachNode!, GetWeaponGrip(skeleton, part.AttachNode!));
    }

    /// <summary>
    ///     Weapons name a WepAbsorpPosParam row, which holds the dummy polys of the character that they sit on.
    ///     Returns null for games without the param, and for weapons that are bound to the skeleton instead.
    /// </summary>
    private static (string Node, Matrix4x4 Offset)? GetAbsorptionAttachment(ProjectEntry project, CharacterPart part, ModelRig skeleton)
    {
        if (GetRow(project, "EquipParamWeapon", part.EquipParamId) is not { } weapon || !HasField(weapon, "absorpParamId") ||
            GetRow(project, "WepAbsorpPosParam", GetValue(weapon, "absorpParamId")) is not { } absorption ||
            GetValue(absorption, "isSkeletonBind") != 0)
        {
            return null;
        }

        var dummyId = GetValue(absorption, IsLeftHand(part) ? "left_0" : "right_0");

        if (GetBodyDummies(project).FirstOrDefault(d => d.ReferenceID == dummyId) is not { } dummy)
        {
            return null;
        }

        var node = dummy.AttachBoneIndex >= 0 ? dummy.AttachBoneIndex : dummy.ParentBoneIndex;

        if (node < 0 || node >= skeleton.NodeNames.Length || dummy.Forward == Vector3.Zero ||
            !Matrix4x4.Invert(skeleton.NodeReferenceWorld[node], out Matrix4x4 inverseNode))
        {
            return null;
        }

        Matrix4x4 world = GetDummyWorld(dummy, skeleton);

        // With tips down -Y, this points blades the way the thumb does. Armored Core has its own convention.
        if (project.Descriptor.ProjectType is not ProjectType.AC6)
        {
            world = WeaponTurn * world;
        }

        return (skeleton.NodeNames[node], world * inverseNode);
    }

    private static Matrix4x4 GetDummyWorld(FLVER.Dummy dummy, ModelRig skeleton)
    {
        Vector3 up = dummy.UseUpwardVector && dummy.Upward != Vector3.Zero ? Vector3.Normalize(dummy.Upward) : Vector3.UnitY;

        Matrix4x4 local = Matrix4x4.CreateWorld(dummy.Position, Vector3.Normalize(dummy.Forward), up);

        return dummy.ParentBoneIndex >= 0 && dummy.ParentBoneIndex < skeleton.NodeReferenceWorld.Length
            ? local * skeleton.NodeReferenceWorld[dummy.ParentBoneIndex]
            : local;
    }

    private static IReadOnlyList<FLVER.Dummy> GetBodyDummies(ProjectEntry project)
    {
        return _bodyDummies.GetValue(project, p =>
        {
            ResourceDescriptor asset = ModelLocator.GetChrModel(p, CompositeModel, CompositeModel);
            var path = asset.AssetVirtualPath != null ? PathBuilder.GetRelativePath(p, asset.AssetVirtualPath) : null;

            try
            {
                if (path != null && p.VFS.FS.FileExists(path) && p.VFS.FS.ReadFile(path) is { } data)
                {
                    using IBinder binder = p.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES
                        ? BND3.Read(data)
                        : BND4.Read(data);

                    if (binder.Files.FirstOrDefault(f => f.Name.Contains(".flv", StringComparison.OrdinalIgnoreCase)) is { } file)
                    {
                        return FLVER2.Read(file.Bytes).Dummies;
                    }
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(CharacterComposition), $"Failed to read the dummy polys of {CompositeModel}", e);
            }

            return new List<FLVER.Dummy>();
        });
    }

    // Used where nothing says how a weapon is held. Weapons point down -Y from the grip, and the front of the
    // skeleton is -Z. The hand bones don't agree between games on which way they face, so ask the bone.
    private static Matrix4x4 GetWeaponGrip(ModelRig skeleton, string node)
    {
        var index = skeleton.FindNode(node);

        if (index < 0)
        {
            return Matrix4x4.Identity;
        }

        return Vector3.TransformNormal(Vector3.UnitY, skeleton.NodeReferenceWorld[index]).Z < 0
            ? WeaponRoll
            : Matrix4x4.Identity;
    }

    // Characters can have upgraded equipment, which uses the row of the base item
    private static Param.Row? FindEquipment(ProjectEntry project, string paramName, int id)
    {
        if (id <= 0)
        {
            return null;
        }

        return GetRow(project, paramName, id)
            ?? GetRow(project, paramName, id - id % 100)
            ?? GetRow(project, paramName, id - id % 10000);
    }

    public static Param.Row? GetRow(ProjectEntry project, string paramName, int id)
    {
        if (id < 0 || project.Handler.ParamData?.PrimaryBank?.Params is not { } bank)
        {
            return null;
        }

        return bank.TryGetValue(paramName, out Param? param) ? param[id] : null;
    }

    // Field names differ in case between the games (e.g. NpcPlayerSex and npcPlayerSex)
    public static bool HasField(Param.Row row, string field)
    {
        return row.Cells.Any(c => c.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase));
    }

    public static float GetFloat(Param.Row row, string field)
    {
        foreach (var cell in row.Cells)
        {
            if (cell.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase))
            {
                return cell.Value is IConvertible value ? value.ToSingle(null) : 0;
            }
        }

        return 0;
    }

    public static int GetValue(Param.Row row, string field)
    {
        foreach (var cell in row.Cells)
        {
            if (cell.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase))
            {
                return cell.Value is IConvertible value ? value.ToInt32(null) : 0;
            }
        }

        return 0;
    }

    // Each piece of equipment names the groups of meshes that it covers, on all of the parts
    private static void HideMasks(Param.Row protector, int[] masks)
    {
        foreach (var cell in protector.Cells)
        {
            Match match = InvisibleFlag.Match(cell.Def.InternalName);

            if (match.Success && int.Parse(match.Groups[1].Value) is var mask && mask < masks.Length &&
                cell.Value is IConvertible value && value.ToInt32(null) != 0)
            {
                masks[mask] = 0;
            }
        }
    }

    private static string[] GetModelGenderLetters(int modelGender, string sex)
    {
        return modelGender switch
        {
            ModelGenderUnisex => new[] { "a", "m" },
            ModelGenderMale => new[] { "m" },
            ModelGenderFemale => new[] { "f" },
            ModelGenderPerSex => new[] { sex, "m" },
            // The male model is used for both, which is how Dark Souls III stores almost all of its armor
            _ => new[] { "m", sex, "a" }
        };
    }
}
