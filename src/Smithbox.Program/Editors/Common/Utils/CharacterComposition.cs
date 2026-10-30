#nullable enable
using Andre.Formats;
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
public sealed record CharacterPart(string Name, string? AttachNode = null);

/// <summary>
///     The part models a character is built from.
/// </summary>
/// <param name="Parts">The part models.</param>
/// <param name="Masks">The enabled material masks of the skinned parts. Null if everything is enabled.</param>
public sealed record CharacterAppearance(IReadOnlyList<CharacterPart> Parts, int[]? Masks);

/// <summary>
///     Builds player-like characters. The FLVER of c0000 only holds a skeleton: what is drawn is the
///     body from the parts folder, and the armor and weapons on top of it come from CharaInitParam.
///     Faces (fg_*) are not displayed yet.
/// </summary>
public static class CharacterComposition
{
    public const string CompositeModel = "c0000";

    // Larger than the invisible flags of any game, as masks past the end of the list are treated as disabled
    private const int MaskCount = 128;

    private const int SexMale = 1;

    private const int ModelGenderUnisex = 0;
    private const int ModelGenderMale = 1;
    private const int ModelGenderFemale = 2;
    private const int ModelGenderPerSex = 3;

    private const string RightHandNode = "R_Weapon";
    private const string LeftHandNode = "L_Weapon";

    // Weapons point down -Y from the grip, which is behind the character in the space of these bones.
    // Rolling around the forearm turns the blade around, and puts the plate of a shield on the outside.
    private static readonly Matrix4x4 WeaponGrip = Matrix4x4.CreateRotationX(MathF.PI);

    // Elden Ring leaves the plain flags empty, and uses the SexVer ones instead
    private static readonly Regex InvisibleFlag = new(@"^invisibleFlag(?:_SexVer)?(\d+)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly ConditionalWeakTable<ProjectEntry, HashSet<string>> _partNames = new();
    private static readonly ConditionalWeakTable<ProjectEntry, ModelRig> _skeletons = new();

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

        AddFirstFound($"fc_{sex}_0100", $"fc_{sex}_0000", "fc_m_0100", "fc_m_0000");

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
                parts.Add(new CharacterPart(name, node));
            }
        }

        AddWeapon("equip_Wep_Right", RightHandNode);
        AddWeapon("equip_Wep_Left", LeftHandNode);

        return new CharacterAppearance(parts, masks);
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
            providers.Add(MeshProviderCache.GetFlverMeshProvider(
                ResourceManager.DefaultPool.Find<FlverResource>(asset.Name),
                part.AttachNode == null ? appearance.Masks : null, uid));

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
            if (appearance.Parts[i].AttachNode is { } attachNode && skeleton.FindNode(attachNode) is >= 0 and var node)
            {
                mesh.Submeshes[i].AttachTransform = WeaponGrip * skeleton.NodeReferenceWorld[node];
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

        for (var i = 0; i < appearance.Parts.Count && i < mesh.Submeshes.Count; i++)
        {
            CharacterPart part = appearance.Parts[i];
            ResourceDescriptor asset = ModelLocator.GetPartsModel(project, part.Name, part.Name);

            parts.Add(new AnimatedPart(part.Name, PathBuilder.GetRelativePath(project, asset.AssetVirtualPath),
                mesh.Submeshes[i], part.AttachNode, WeaponGrip));
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

    private static Param.Row? GetRow(ProjectEntry project, string paramName, int id)
    {
        if (id < 0 || project.Handler.ParamData?.PrimaryBank?.Params is not { } bank)
        {
            return null;
        }

        return bank.TryGetValue(paramName, out Param? param) ? param[id] : null;
    }

    // Field names differ in case between the games (e.g. NpcPlayerSex and npcPlayerSex)
    private static bool HasField(Param.Row row, string field)
    {
        return row.Cells.Any(c => c.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase));
    }

    private static int GetValue(Param.Row row, string field)
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
