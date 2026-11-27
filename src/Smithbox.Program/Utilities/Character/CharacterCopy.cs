#nullable enable
using Andre.IO.VFS;
using Havok;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Interface;
using StudioCore.Renderer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Utilities;

/// <summary>One file a character copy writes: read from Source, written as Target</summary>
public sealed record CharacterCopyFile(string Source, string Target, string Kind);

/// <summary>What copying a character under a new ID reads and writes (Character Scale Plan.md P6, D2, D6)</summary>
public sealed class CharacterCopyPlan
{
    public ProjectType Game;
    public string Source = "";
    public string Target = "";

    /// <summary>The character whose clips the source plays (normalChangeAnimChrId), or the source itself</summary>
    public string AnimationSource = "";

    public List<CharacterCopyFile> Files = new();
    public List<string> Refusals = new();
    public List<string> Notes = new();
    public bool CanApply => Refusals.Count == 0;
}

/// <summary>
///     Copies a character's files under a new ID, renamed inside (binder file names, Havok strings, the time act's
///     ID and names, sound bank IDs), so the copy can be scaled without touching the original (D2). Its own files
///     are copied: chrbnd, anibnds, behbnd, HKS script, sound and effect banks (D6). Textures stay shared through
///     NpcParam normalChangeTexChrId. A character that plays another's clips gets that character's anibnds as its own
///     (when the two skeletons have the same bones), so its clips can be scaled too.
/// </summary>
public static class CharacterCopy
{
    private static readonly Regex Id = new(@"^c\d{4}$", RegexOptions.IgnoreCase);

    public static int Number(string character) => int.Parse(character.AsSpan(1));

    /// <summary>Whether a character ID is free: no chrbnd under it in the project or the game</summary>
    public static bool IsFree(VirtualFileSystem fs, string character) =>
        !fs.FileExists(CharacterScale.ChrbndPath(character)) && !fs.FileExists($"/chr/{character}.chrbnd");

    /// <summary>The first free ID after the source (c2010 → c2011…), whose NpcParam row block is free too</summary>
    public static string? SuggestId(VirtualFileSystem fs, string source, Func<int, bool>? rowBlockFree = null)
    {
        for (var n = Number(source) + 1; n <= 9999; n++)
        {
            var id = $"c{n:D4}";
            if (IsFree(fs, id) && (rowBlockFree?.Invoke(n) ?? true))
                return id;
        }

        return null;
    }

    /// <summary>The files to copy, and why it can't be done (if so)</summary>
    /// <param name="animationSource">The character whose clips the source plays (NpcParam normalChangeAnimChrId), or null</param>
    public static CharacterCopyPlan CreatePlan(VirtualFileSystem fs, ProjectType game, string source, string target,
        string? animationSource)
    {
        source = source.ToLowerInvariant();
        target = target.ToLowerInvariant();
        var plan = new CharacterCopyPlan
        {
            Game = game,
            Source = source,
            Target = target,
            AnimationSource = (animationSource ?? source).ToLowerInvariant()
        };

        if (!Id.IsMatch(target))
            plan.Refusals.Add(LOC.Get("CHRSCALE_Copy_Bad_Id", target));
        else if (target == source)
            plan.Refusals.Add(LOC.Get("CHRSCALE_Copy_Same_Id"));
        else if (!IsFree(fs, target))
            plan.Refusals.Add(LOC.Get("CHRSCALE_Copy_Taken", target));

        void Add(string from, string kind, string? fromId = null)
        {
            var name = Path.GetFileName(from);
            var to = from[..^name.Length] + name.Replace(fromId ?? source, target, StringComparison.OrdinalIgnoreCase);
            plan.Files.Add(new CharacterCopyFile(from, to, kind));
        }

        var chrbnd = CharacterScale.ChrbndPath(source);
        if (!fs.FileExists(chrbnd))
        {
            plan.Refusals.Add(LOC.Get("CHRSCALE_Refuse_No_Chrbnd", chrbnd));
            return plan;
        }

        Add(chrbnd, "chrbnd");

        // Clips: its own anibnds, or those of the character it borrows from (with that character's skeleton)
        var ownAnibnds = AnimationPackage.FindCharacterBinders(fs, chrbnd, source).Select(p => p.Replace('\\', '/')).ToList();
        if (plan.AnimationSource != source)
        {
            var borrowed = AnimationPackage.FindCharacterBinders(fs, CharacterScale.ChrbndPath(plan.AnimationSource), plan.AnimationSource)
                .Select(p => p.Replace('\\', '/')).ToList();
            if (borrowed.Count == 0)
            {
                plan.Refusals.Add(LOC.Get("CHRSCALE_Copy_No_Borrowed", plan.AnimationSource));
            }
            else if (!SameBones(fs, game, ownAnibnds.FirstOrDefault(), borrowed[0], source, plan.AnimationSource))
            {
                plan.Refusals.Add(LOC.Get("CHRSCALE_Copy_Rig_Differs", source, plan.AnimationSource));
            }
            else
            {
                foreach (var path in borrowed)
                    Add(path, "anibnd", plan.AnimationSource);
                plan.Notes.Add(LOC.Get("CHRSCALE_Copy_Borrowed_Note", plan.AnimationSource, target));
            }
        }
        else
        {
            foreach (var path in ownAnibnds)
                Add(path, "anibnd");
        }

        var behbnd = CharacterScale.BehbndPath(source);
        if (fs.FileExists(behbnd))
            Add(behbnd, "behbnd");

        // HKS: its own, else the clip source's (a variant often has none)
        foreach (var who in new[] { source, plan.AnimationSource }.Distinct())
        {
            var hks = $"/action/script/{who}.hks";
            if (!fs.FileExists(hks))
                continue;
            plan.Files.Add(new CharacterCopyFile(hks, $"/action/script/{target}.hks", "hks"));
            break;
        }

        // Sound and effect banks (D6): every file named after the character in the sound and sfx folders
        foreach (var path in Siblings(fs, source))
            Add(path, path.Contains("/sfx/", StringComparison.OrdinalIgnoreCase) ? "sfx" : "sound");

        plan.Notes.Add(LOC.Get("CHRSCALE_Copy_Textures_Note", source));
        return plan;
    }

    // Sound banks (ER /cs_cXXXX.bnk, BB /sound/sprj_cXXXX.*) and effect binders (ER /sfx/sfxbnd_cXXXX.ffxbnd)
    private static IEnumerable<string> Siblings(VirtualFileSystem fs, string source)
    {
        var found = new List<string>();
        foreach (var path in new[] { $"/cs_{source}.bnk", $"/sd/cs_{source}.bnk", $"/sfx/sfxbnd_{source}.ffxbnd.dcx",
                     $"/sound/sprj_{source}.fev", $"/sound/sprj_{source}.fsb", $"/sound/sprj_{source}.itl" })
            if (fs.FileExists(path))
                found.Add(path);
        return found;
    }

    /// <summary>
    ///     Whether the borrowed set's skeleton has the source's bones, in the same order with the same parents (their
    ///     names compared without the characters' IDs: c4101_Skirt_005 is c4100_Skirt_005)
    /// </summary>
    private static bool SameBones(VirtualFileSystem fs, ProjectType game, string? ownAnibnd, string borrowedAnibnd,
        string source, string animationSource)
    {
        var own = ReadSkeleton(fs, game, ownAnibnd);
        var other = ReadSkeleton(fs, game, borrowedAnibnd);
        // No skeleton of its own: it has nothing for the borrowed clips to differ from
        if (own == null)
            return true;
        if (other == null || own.BoneCount != other.BoneCount || !own.Parents.SequenceEqual(other.Parents))
            return false;

        // Both IDs out of both lists: a variant is often rigged with its source's names (c4101's c4100_Skirt_005)
        string Plain(string name) => name.Replace(source, "cXXXX", StringComparison.OrdinalIgnoreCase)
            .Replace(animationSource, "cXXXX", StringComparison.OrdinalIgnoreCase);
        return own.Names.Select(Plain).SequenceEqual(other.Names.Select(Plain), StringComparer.OrdinalIgnoreCase);
    }

    public static HavokSkeletonData? ReadSkeleton(VirtualFileSystem fs, ProjectType game, string? anibnd)
    {
        if (anibnd == null || fs.ReadFile(anibnd) is not { } data)
            return null;
        using var reader = AnimationPackage.OpenBinder(game, data);
        var ids = reader.Files.Select(f => f.ID).ToList();
        var skeletonId = AnimationPackage.SkeletonBinderId(ids);
        if (reader.Files.FirstOrDefault(f => f.ID == skeletonId) is not { } header)
            return null;
        Havok.HavokCompendium? compendium = null;
        if (reader.Files.FirstOrDefault(f => f.ID == AnimationPackage.CompendiumBinderId) is { } c)
            compendium = HavokAnimationReader.ReadCompendium(game, reader.ReadFile(c).ToArray());
        return HavokAnimationReader.ReadSkeleton(game, reader.ReadFile(header).ToArray(), compendium);
    }

    /// <summary>Writes every file of the plan, renamed inside. Returns the paths written.</summary>
    public static List<string> Apply(CharacterCopyPlan plan, VirtualFileSystem fs, Action<string, byte[]> write,
        List<string> failures)
    {
        var written = new List<string>();
        if (!plan.CanApply)
        {
            failures.AddRange(plan.Refusals);
            return written;
        }

        foreach (var file in plan.Files)
        {
            try
            {
                var data = fs.ReadFileOrThrow(file.Source).ToArray();
                var from = file.Kind == "anibnd" ? plan.AnimationSource : plan.Source;
                var bytes = file.Kind switch
                {
                    "chrbnd" or "anibnd" or "behbnd" or "sfx" => RenameBinder(plan.Game, data, from, plan.Target, file.Kind),
                    "sound" => RenameSoundBank(data, file.Source, from, plan.Target),
                    _ => data
                };
                write(file.Target, bytes);
                written.Add(file.Target);
            }
            catch (Exception e)
            {
                failures.Add($"{file.Source}: {e.Message}");
            }
        }

        return written;
    }

    /// <summary>
    ///     A binder with the ID renamed: its files' names; the Havok strings of its character, physics, cloth,
    ///     behavior and skeleton files (not every clip: their strings don't name the character); its time acts' ID
    ///     (ER: 200000 + the character) and skeleton / sibling names
    /// </summary>
    public static byte[] RenameBinder(ProjectType game, byte[] data, string from, string to, string kind)
    {
        IBinder bnd = AnimationPackage.UsesLegacyBinders(game) ? BND3.Read(data) : BND4.Read(data);
        var profile = HavokAnimationReader.ProfileOf(game);
        Havok.HavokCompendium? compendium = null;
        if (bnd.Files.FirstOrDefault(f => f.Name.EndsWith(".compendium", StringComparison.OrdinalIgnoreCase)) is { } c)
            compendium = HavokAnimationReader.ReadCompendium(game, c.Bytes.ToArray());

        var fromNumber = Number(from);
        var toNumber = Number(to);
        foreach (var file in bnd.Files)
        {
            file.Name = file.Name.Replace(from, to, StringComparison.OrdinalIgnoreCase);
            var name = Path.GetFileName(file.Name.Replace('\\', '/')).ToLowerInvariant();

            if (name.EndsWith(".tae"))
            {
                var (raw, dcx) = Unpack(file.Bytes);
                var tae = TAE.Read(raw);
                tae.SkeletonName = tae.SkeletonName?.Replace(from, to, StringComparison.OrdinalIgnoreCase);
                tae.SibName = tae.SibName?.Replace(from, to, StringComparison.OrdinalIgnoreCase);
                if (tae.ID % 10000 == fromNumber)
                    tae.ID += toNumber - fromNumber;
                file.Bytes = Pack(tae.Write(), dcx);
            }
            // Havok strings only in the behbnd, whose project and character files name the renamed files. Skeletons,
            // ragdolls and cloth keep their names: bone names carry the ID (c4100_Skirt_005) and the FLVER's bones and
            // the clips' bindings keep theirs, so renaming them could part the model from its skeleton
            else if (name.EndsWith(".hkx") && IsBehaviorFile(file.Name))
            {
                var (raw, dcx) = Unpack(file.Bytes);
                try
                {
                    var document = Havok.Files.HavokFile.Read(raw, profile, compendium);
                    if (RenameStrings(document, from, to) > 0)
                        file.Bytes = Pack(Havok.Files.HavokFile.Write(document), dcx);
                }
                catch (Exception)
                {
                    // A file the reader doesn't take (DS3's legacy .hkt copies are .hkt, not .hkx) stays as it is
                }
            }
        }

        return bnd switch
        {
            BND3 b3 => b3.Write(),
            BND4 b4 => b4.Write(),
            _ => throw new NotSupportedException(bnd.GetType().Name)
        };
    }

    /// <summary>A behavior project file (behaviors/, characters/, export/): in the behbnd, or in BB the anibnd</summary>
    public static bool IsBehaviorFile(string name)
    {
        var path = name.Replace('\\', '/').ToLowerInvariant();
        return path.Contains("/behaviors/") || path.Contains("/characters/") || path.Contains("/export/");
    }

    /// <summary>Every string of a Havok document that names the character, renamed. Returns how many.</summary>
    public static int RenameStrings(HavokDocument document, string from, string to)
    {
        var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var count = 0;
        foreach (var o in document.Objects)
            count += Walk(o, from, to, seen);
        return count;
    }

    private static int Walk(object? o, string from, string to, HashSet<object> seen)
    {
        if (o == null || o is string || o.GetType().IsPrimitive || o.GetType().IsEnum)
            return 0;
        if (!o.GetType().IsValueType && !seen.Add(o))
            return 0;
        // Bone names stay (see RenameBinder)
        if (o.GetType().Name is "hkaBone" or "hkaSkeleton")
            return 0;

        var count = 0;
        if (o is IList list)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (list[i] is string s && s.Contains(from, StringComparison.OrdinalIgnoreCase))
                {
                    list[i] = s.Replace(from, to, StringComparison.OrdinalIgnoreCase);
                    count++;
                }
                else
                {
                    count += Walk(list[i], from, to, seen);
                }
            }
            return count;
        }

        if (o.GetType().Namespace?.StartsWith("Havok") != true)
            return 0;

        foreach (var f in o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var v = f.GetValue(o);
            if (v is string s)
            {
                if (s.Contains(from, StringComparison.OrdinalIgnoreCase) && !f.IsInitOnly)
                {
                    f.SetValue(o, s.Replace(from, to, StringComparison.OrdinalIgnoreCase));
                    count++;
                }
            }
            else if (v != null && f.FieldType.IsValueType && !f.FieldType.IsPrimitive && !f.FieldType.IsEnum)
            {
                count += Walk(v, from, to, seen);
                if (!f.IsInitOnly)
                    f.SetValue(o, v);
            }
            else
            {
                count += Walk(v, from, to, seen);
            }
        }

        return count;
    }

    /// <summary>
    ///     A sound bank under the new name. ER's Wwise banks carry their ID (FNV-1 of "cs_cXXXX") at BKHD + 12; BB's
    ///     FMOD files name their project ("sprj_cXXXX"), renamed byte for byte (the same length)
    /// </summary>
    public static byte[] RenameSoundBank(byte[] data, string path, string from, string to)
    {
        var copy = (byte[])data.Clone();
        if (path.EndsWith(".bnk", StringComparison.OrdinalIgnoreCase) && copy.Length >= 16 &&
            Encoding.ASCII.GetString(copy, 0, 4) == "BKHD")
        {
            BitConverter.GetBytes(Fnv1($"cs_{to}")).CopyTo(copy, 12);
            return copy;
        }

        if (path.EndsWith(".fev", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".itl", StringComparison.OrdinalIgnoreCase))
        {
            var a = Encoding.ASCII.GetBytes($"sprj_{from}");
            var b = Encoding.ASCII.GetBytes($"sprj_{to}");
            ReplaceAll(copy, a, b);
            ReplaceAll(copy, Encoding.ASCII.GetBytes($"SPRJ_{from.ToUpperInvariant()}"), Encoding.ASCII.GetBytes($"SPRJ_{to.ToUpperInvariant()}"));
        }

        return copy;
    }

    private static void ReplaceAll(byte[] data, byte[] a, byte[] b)
    {
        for (var i = 0; i + a.Length <= data.Length; i++)
            if (data.AsSpan(i, a.Length).SequenceEqual(a))
                b.CopyTo(data, i);
    }

    /// <summary>FNV-1 32 of a lower-case name (Wwise bank and object IDs)</summary>
    public static uint Fnv1(string s)
    {
        uint h = 2166136261;
        foreach (var c in s.ToLowerInvariant())
        {
            h *= 16777619;
            h ^= (byte)c;
        }

        return h;
    }

    private static (byte[] Raw, DCX.Type Dcx) Unpack(Memory<byte> data)
    {
        if (!DCX.Is(data))
            return (data.ToArray(), DCX.Type.None);
        var raw = DCX.Decompress(data.ToArray(), out var type);
        return (raw.ToArray(), type);
    }

    private static byte[] Pack(byte[] raw, DCX.Type dcx) => dcx == DCX.Type.None ? raw : DCX.Compress(raw, dcx).ToArray();
}
