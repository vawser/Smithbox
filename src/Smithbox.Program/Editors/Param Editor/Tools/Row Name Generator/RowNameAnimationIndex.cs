using Havok;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.HavokEditor;
using StudioCore.Editors.TimeActEditor;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// What a character's animations say about its behaviors, for the row name generator:
/// the BehaviorJudgeID of every attack / bullet / throw event in its time acts (judge -> animations),
/// and the Havok behavior state that plays each animation (CMSG m_animId -> owning state's name).
/// </summary>
public class CharacterAnimationInfo
{
    public string Character;
    public int CategorySize;

    /// <summary>Judge IDs added to the attacker's variation -> full animation IDs, lowest first</summary>
    public Dictionary<int, List<long>> VariationJudges = new();

    /// <summary>Judge IDs that are a row ID as they are ("Common" in the time act meta) -> full animation IDs</summary>
    public Dictionary<int, List<long>> CommonJudges = new();

    /// <summary>CMSG animation ID -> the name of the state it plays in (or the CMSG's own name)</summary>
    public Dictionary<long, string> States = new();

    public string GetStateName(long fullAnimationId)
    {
        if (States.TryGetValue(fullAnimationId, out var name))
            return name;

        // The player's CMSGs hold the ID within the category (a000_003000 -> 3000)
        if (States.TryGetValue(fullAnimationId % CategorySize, out name))
            return name;

        return null;
    }

    public string FormatAnimationName(long fullAnimationId)
    {
        return CategorySize == 10000
            ? $"a{fullAnimationId / CategorySize:D2}_{fullAnimationId % CategorySize:D4}"
            : $"a{fullAnimationId / CategorySize:D3}_{fullAnimationId % CategorySize:D6}";
    }
}

/// <summary>
/// Progress of a background load, polled by the tool window
/// </summary>
public class RowNameAnimationLoad
{
    public int Total;
    public int Completed;
    public volatile string Current = "";
    public volatile bool IsDone;
    public CancellationTokenSource Cancel = new();
}

/// <summary>
/// Reads the time acts (anibnd) and Havok behaviors (behbnd) of characters in the background, kept per project for the
/// session so a second Generate doesn't read them again. Read from the stored files: unsaved Time Act and Behavior
/// Editor edits aren't included.
/// </summary>
public static class RowNameAnimationIndex
{
    private const string JudgeParameter = "BehaviorJudgeID";

    private static readonly ConditionalWeakTable<ProjectEntry, ConcurrentDictionary<string, CharacterAnimationInfo>> Cache = new();

    private static ConcurrentDictionary<string, CharacterAnimationInfo> GetCache(ProjectEntry project)
    {
        return Cache.GetValue(project, _ => new ConcurrentDictionary<string, CharacterAnimationInfo>(StringComparer.OrdinalIgnoreCase));
    }

    /// <summary>The loaded info for a character, or null if it hasn't been loaded (or it has no time act)</summary>
    public static CharacterAnimationInfo Get(ProjectEntry project, string character)
    {
        if (project == null || character == null)
            return null;

        return GetCache(project).TryGetValue(character, out var info) ? info : null;
    }

    public static bool IsLoaded(ProjectEntry project, string character)
    {
        return project != null && character != null && GetCache(project).ContainsKey(character);
    }

    /// <summary>Forget everything read, so the next Generate reads the files again (after editing time acts)</summary>
    public static void Clear(ProjectEntry project)
    {
        if (project != null && Cache.TryGetValue(project, out var cache))
            cache.Clear();
    }

    /// <summary>Whether the character has a time act in the project (cXXXX in /chr)</summary>
    public static bool HasTimeAct(ProjectEntry project, string character)
    {
        return FindTimeActEntry(project, character) != null;
    }

    /// <summary>
    /// Starts reading the characters that aren't loaded yet. Must be called on the UI thread: it reads the time act
    /// template there, as <see cref="TimeActData"/> caches it in a dictionary that isn't safe to fill from a worker.
    /// </summary>
    public static RowNameAnimationLoad StartLoad(ProjectEntry project, IEnumerable<string> characters)
    {
        var load = new RowNameAnimationLoad();
        var cache = GetCache(project);
        var pending = characters.Where(c => !cache.ContainsKey(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        load.Total = pending.Count;

        var timeActData = project.Handler.TimeActData;

        if (pending.Count == 0 || timeActData == null)
        {
            load.IsDone = true;
            return load;
        }

        // Every character time act uses the same template; read it here, on the UI thread
        TAE.Template.BankTemplate template = null;
        var firstEntry = pending.Select(c => FindTimeActEntry(project, c)).FirstOrDefault(e => e != null);

        if (firstEntry != null)
            template = timeActData.GetTemplate(new TimeActContainerWrapper(project, firstEntry, null, project.VFS.FS));

        if (template == null)
        {
            load.IsDone = true;
            return load;
        }

        var token = load.Cancel.Token;

        Task.Run(() =>
        {
            try
            {
                var havokBank = new HavokBank(project);
                var options = new ParallelOptions
                {
                    // Each read decompresses a whole anibnd, so not every core at once
                    MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount / 2),
                    CancellationToken = token
                };

                Parallel.ForEach(pending, options, character =>
                {
                    load.Current = character;

                    try
                    {
                        var info = ReadCharacter(project, timeActData, template, havokBank, character, token);

                        if (info != null && !token.IsCancellationRequested)
                            cache[character] = info;
                    }
                    catch (Exception e)
                    {
                        Smithbox.LogError(typeof(RowNameAnimationIndex), LOC.Get("PARAM_RowNameGen_Log_Animation_Read_Failed", character), e);
                    }

                    Interlocked.Increment(ref load.Completed);
                });
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(RowNameAnimationIndex), LOC.Get("PARAM_RowNameGen_Log_Animation_Load_Failed"), e);
            }
            finally
            {
                load.IsDone = true;
            }
        });

        return load;
    }

    private static FileDictionaryEntry FindTimeActEntry(ProjectEntry project, string character)
    {
        return project.Locator.TimeActFiles.Entries.FirstOrDefault(e =>
            e.Folder.StartsWith("/chr", StringComparison.OrdinalIgnoreCase) &&
            e.Filename.Equals(character, StringComparison.OrdinalIgnoreCase));
    }

    private static CharacterAnimationInfo ReadCharacter(ProjectEntry project, TimeActData timeActData,
        TAE.Template.BankTemplate template, HavokBank havokBank, string character, CancellationToken token)
    {
        var entry = FindTimeActEntry(project, character);

        if (entry == null)
            return null;

        var wrapper = new TimeActContainerWrapper(project, entry, null, project.VFS.FS);

        var info = new CharacterAnimationInfo
        {
            Character = character,
            CategorySize = wrapper.AnimationCategorySize
        };

        // Time acts: judge IDs of the events
        foreach (var timeAct in wrapper.ReadTimeActs())
        {
            if (token.IsCancellationRequested)
                return null;

            timeAct.Load();

            if (timeAct.TimeAct == null)
                continue;

            foreach (var animation in timeAct.TimeAct.Animations)
            {
                timeAct.ApplyTemplate(animation, template);

                var fullId = timeAct.GetFullAnimationID(animation, info.CategorySize);

                foreach (var ev in animation.Events)
                {
                    if (ev.Parameters == null)
                        continue;

                    if (!ev.Parameters.Values.TryGetValue(JudgeParameter, out var raw) || raw is not IConvertible number)
                        continue;

                    int judge;

                    try
                    {
                        judge = number.ToInt32(null);
                    }
                    catch (OverflowException)
                    {
                        continue;
                    }

                    if (judge < 0)
                        continue;

                    var common = timeActData.GetFieldMeta(ev, JudgeParameter)?.BehaviorRef == "Common";
                    var target = common ? info.CommonJudges : info.VariationJudges;

                    if (!target.TryGetValue(judge, out var list))
                    {
                        list = new List<long>();
                        target[judge] = list;
                    }

                    if (!list.Contains(fullId))
                        list.Add(fullId);
                }
            }
        }

        foreach (var list in info.VariationJudges.Values.Concat(info.CommonJudges.Values))
            list.Sort();

        // Havok behaviors: the state each animation plays in
        var behbnd = project.Locator.HavokBehaviorFiles.Entries.FirstOrDefault(e =>
            e.Filename.Equals(character, StringComparison.OrdinalIgnoreCase));

        if (behbnd != null && HavokTypeUtils.GetProfile(project) != null)
        {
            // States per file (each document is dropped once read), then merged with the character's own graph
            // first, so its states win over the shared c9997 / c0000 ones
            var perFile = new List<(bool Own, Dictionary<long, string> States)>();

            havokBank.ReadStoredBehaviorFiles(behbnd, false, _ => true, (name, document) =>
            {
                var fileName = System.IO.Path.GetFileName(name.Replace('\\', '/'));
                perFile.Add((fileName.StartsWith(character, StringComparison.OrdinalIgnoreCase), ReadStates(document)));
            }, token);

            foreach (var (_, states) in perFile.OrderBy(f => f.Own ? 0 : 1))
            {
                foreach (var (animId, name) in states)
                    info.States.TryAdd(animId, name);
            }
        }

        return info;
    }

    private static Dictionary<long, string> ReadStates(HavokDocument document)
    {
        var states = new Dictionary<long, string>();

        if (document?.Root == null)
            return states;

        // A field cache of its own: the editor's isn't thread-safe
        var cache = new HavokPropertyCache { ProfileSource = () => document.Profile };
        var symbols = HavokBehaviorSymbols.Build(document.Root, cache.GetCachedHavokFields);

        if (symbols == null)
            return states;

        foreach (var selector in symbols.Selectors)
        {
            var animIdValue = HavokBehaviorSymbols.GetValue(selector, "m_animId");

            if (animIdValue is not IConvertible animIdNumber)
                continue;

            long animId;

            try
            {
                animId = animIdNumber.ToInt64(null);
            }
            catch (OverflowException)
            {
                continue;
            }

            if (animId < 0 || states.ContainsKey(animId))
                continue;

            var state = symbols.FindOwningState(selector);
            var name = state != null ? HavokBehaviorSymbols.GetValue(state, "m_name") as string : null;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = HavokBehaviorSymbols.GetValue(selector, "m_name") as string;

                if (name != null && name.EndsWith("_CMSG", StringComparison.OrdinalIgnoreCase))
                    name = name[..^5];
            }

            if (!string.IsNullOrWhiteSpace(name))
                states[animId] = name.Trim();
        }

        return states;
    }
}
