using StudioCore.Editors.Common;
using System.Collections;

namespace StudioCore.Editors.HavokEditor;

/// <summary>
/// The animation names a behbnd's character file lists (Characters/cXXXX.hkx, hkbCharacterStringData
/// .m_animationBundleNameData[0].m_assetNames), and the file, so an edit to the list marks it changed and it's saved
/// with the binder
/// </summary>
public record HavokCharacterAnimationList(HavokFileWrapper File, IList Names);

/// <summary>
/// DS3 (HKX2) clips have no internal animation ID: the character file lists every animation the behavior plays, by
/// name (BehaviorSurvey chardata: one bundle per character file, one name per distinct clip animation in c0000).
/// A clip for an animation the list lacks gets the name added there, in the same undo step.
/// </summary>
public static partial class HavokBehaviorOperations
{
    #region Character file
    /// <summary>
    /// Makes sure the character file lists the animation: true if it does already, or a step that adds it was added
    /// to steps (once per edit: addedNames remembers it). False with the reason when there's no character file list.
    /// </summary>
    private static bool EnsureCharacterAnimation(HavokBehaviorContext context, string animation, List<EditorAction> steps,
        Dictionary<string, int> addedNames, out string error)
    {
        error = null;

        if (addedNames != null && addedNames.ContainsKey(animation))
            return true;

        var list = context.Character?.Invoke();

        if (list?.Names == null || list.File == null)
        {
            error = LOC.Get("HAVOK_BehaviorActions_Error_Character_Bundle", animation);
            return false;
        }

        var names = list.Names;

        if (names.Cast<object>().Any(n => string.Equals(n as string, animation, StringComparison.OrdinalIgnoreCase)))
            return true;

        // On the character file: it's marked changed, and saved with the binder
        steps.Add(new HavokDelegateAction(list.File, () => names.Add(animation), () => names.RemoveAt(names.Count - 1)));

        if (addedNames != null)
            addedNames[animation] = -1;

        return true;
    }

    /// <summary>
    /// The character file animation list of a behbnd in the bank, loading the character file if needed; null if the
    /// binder has no character file or it has no animation bundle
    /// </summary>
    public static HavokCharacterAnimationList FindCharacterAnimations(HavokBank bank, FileDictionaryEntry binder, Func<Type, System.Reflection.FieldInfo[]> fields)
    {
        if (binder == null || bank.BehaviorBank.GetValueOrDefault(binder) is not { } files)
            return null;

        var key = files.Keys.FirstOrDefault(k => k.Replace('\\', '/').Contains("/Characters/", StringComparison.OrdinalIgnoreCase)
            && k.Contains(".hkx", StringComparison.OrdinalIgnoreCase));

        if (key == null)
            return null;

        bank.LoadBehaviorFile(binder, key);

        var wrapper = files.GetValueOrDefault(key);

        if (wrapper?.Root == null)
            return null;

        var stringData = HavokTreeSearch.FindAllOfClass(wrapper.Root, "hkbCharacterStringData", fields).FirstOrDefault();
        var bundles = HavokBehaviorSymbols.GetValue(stringData, "m_animationBundleNameData") as IList;

        if (bundles == null || bundles.Count == 0)
            return null;

        return HavokBehaviorSymbols.GetValue(bundles[0], "m_assetNames") is IList names ? new(wrapper, names) : null;
    }
    #endregion
}
