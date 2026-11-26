#nullable enable
using SoulsFormats;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The animations a character's Havok behavior files name (Animation Editor Analysis.md §8, W15), for the Clips list's
/// Used Only filter. A clip generator stores its animation as a name string (<c>m_animationName</c>, aXXX_YYYYYY), and
/// the graph's string data lists them too, so the files are searched for such names rather than read as behavior
/// graphs (which the Behavior Editor does, at far more cost). A name a file holds for another reason would count as
/// used too; that errs on the side of showing a clip.
/// </summary>
public static class AnimationBehaviorUse
{
    private static readonly Regex ClipName = new(@"a(\d{3})_(\d{6})", RegexOptions.Compiled);

    /// <summary>
    /// The animation IDs named by the behavior binders of the characters (the character and the one whose set it
    /// plays), or null when none of them has a behavior binder (so nothing can be said). Read off the main thread.
    /// </summary>
    public static HashSet<long>? Read(ProjectEntry project, IEnumerable<string> characters)
    {
        var names = characters.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var entries = project.Locator.HavokBehaviorFiles.Entries
            .Where(e => names.Any(n => e.Filename.Equals(n, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (entries.Count == 0)
            return null;

        var used = new HashSet<long>();
        foreach (var entry in entries)
        {
            try
            {
                if (project.VFS.FS.ReadFile(entry.Path) is not { } data)
                    continue;

                var binder = BND4.Read(data);
                foreach (var file in binder.Files)
                {
                    if (!file.Name.EndsWith(".hkx", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var bytes = file.Bytes.ToArray();
                    if (DCX.Is(bytes))
                        bytes = DCX.Decompress(bytes, out _).ToArray();

                    // Latin-1 keeps one character per byte, so the names read as they are stored
                    foreach (Match m in ClipName.Matches(Encoding.Latin1.GetString(bytes)))
                        used.Add(long.Parse(m.Groups[1].Value) * 1000000 + long.Parse(m.Groups[2].Value));
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(AnimationBehaviorUse), LOC.Get("ANIM_Used_Behavior_Failed", entry.Path), e);
            }
        }

        return used;
    }
}
