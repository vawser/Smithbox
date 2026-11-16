using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


/* Restricted characters: colon, space, forward slash, ampersand, exclamation mark
 *
 */
public class SearchEngine<A, B>
{
    internal SearchEngineCommand<A, B> defaultFilter;

    internal Dictionary<string, SearchEngineCommand<A, B>> filterList = new();
    internal Func<A, List<B>> unpacker;


    protected void addExistsFilter()
    {
        filterList.Add("exists", newCmd(new string[0], 
            LOC.Get("PARAM_SE_Exists_TT"), 
            noArgs(noContext(B => true))));
    }

    protected Func<string[], bool, Func<A, Func<B, bool>>> noArgs(Func<A, Func<B, bool>> func)
    {
        return (args, lenient) => func;
    }

    protected Func<A, Func<B, bool>> noContext(Func<B, bool> func)
    {
        return context => func;
    }


    internal SearchEngineCommand<A, B> newCmd(string[] args, string wiki,
        Func<string[], bool, Func<A, Func<B, bool>>> func, Func<bool> shouldShow = null)
    {
        return new SearchEngineCommand<A, B>(args, wiki, func, shouldShow);
    }

    public bool HandlesCommand(string command)
    {
        if (command.Length > 0 && command.StartsWith('!'))
        {
            command = command.Substring(1);
        }

        return filterList.ContainsKey(command.Split(" ")[0]);
    }

    public List<string> AvailableCommandsForHelpText()
    {
        List<string> options = new();
        foreach (var op in filterList.Keys)
        {
            SearchEngineCommand<A, B> cmd = filterList[op];
            if (cmd.shouldShow == null || cmd.shouldShow())
            {
                options.Add(
                    LOC.Get("PARAM_SE_CommandsForHelp_Args", op, filterList[op].args.Length));
            }
        }

        if (defaultFilter != null && (defaultFilter.shouldShow == null || defaultFilter.shouldShow()))
        {
            options.Add(LOC.Get("PARAM_SE_CommandsForHelp_Default_Args", defaultFilter.args.Length));
        }

        return options;
    }

    public List<(string, string[], string)> VisibleCommands()
    {
        List<(string, string[], string)> options = new();
        foreach (var op in filterList.Keys)
        {
            SearchEngineCommand<A, B> cmd = filterList[op];
            if (cmd.shouldShow == null || cmd.shouldShow())
            {
                options.Add((op, cmd.args, cmd.wiki));
            }
        }

        return options;
    }

    public List<(string, string[])> AllCommands()
    {
        List<(string, string[])> options = new();
        foreach (var op in filterList.Keys)
        {
            options.Add((op, filterList[op].args));
        }

        if (defaultFilter != null)
        {
            options.Add(("", defaultFilter.args));
        }

        return options;
    }

    public List<B> Search(A param, string command, bool lenient, bool failureAllOrNone)
    {
        return Search(param, unpacker(param), command, lenient, failureAllOrNone);
    }

    public virtual List<B> Search(A context, List<B> sourceSet, string command, bool lenient, bool failureAllOrNone)
    {
        //assumes unpacking doesn't fail
        var conditions = command.Split("&&", StringSplitOptions.TrimEntries);
        List<B> liveSet = sourceSet;
        string currentCondition = null;

        try
        {
            foreach (var condition in conditions)
            {
                //temp
                if (condition.Equals(""))
                {
                    break;
                }

                currentCondition = condition;

                var cmd = condition.Split(' ', 2);

                SearchEngineCommand<A, B> selectedCommand;
                int argC;
                string[] args;
                var not = false;
                if (cmd[0].Length > 0 && cmd[0].StartsWith('!'))
                {
                    cmd[0] = cmd[0].Substring(1);
                    not = true;
                }

                if (filterList.ContainsKey(cmd[0]))
                {
                    selectedCommand = filterList[cmd[0]];
                    argC = selectedCommand.args.Length;
                    args = cmd.Length == 1
                        ? new string[0]
                        : cmd[1].Split(' ', argC, StringSplitOptions.TrimEntries);
                }
                else
                {
                    selectedCommand = defaultFilter;
                    argC = selectedCommand.args.Length;
                    args = condition.Split(" ", argC, StringSplitOptions.TrimEntries);
                }

                // Say what's missing, rather than failing on an index out of range
                if (args.Length < argC)
                {
                    throw new Exception(LOC.Get("PARAM_SE_Error_Missing_Args",
                        cmd[0], argC, string.Join(", ", selectedCommand.args)));
                }

                for (var i = 0; i < argC; i++)
                {
                    if (args[i].StartsWith('$'))
                    {
                        var varName = args[i].Substring(1);

                        if (!MassParamEdit.massEditVars.TryGetValue(varName, out var varValue))
                        {
                            throw new Exception(LOC.Get("PARAM_SE_Error_Unknown_Variable", varName));
                        }

                        args[i] = varValue.ToString();
                    }
                }

                Func<A, Func<B, bool>> filter = selectedCommand.func(args, lenient);
                Func<B, bool> criteria = filter(context);
                List<B> newRows = new();
                foreach (B row in liveSet)
                {
                    if (not ^ criteria(row))
                    {
                        newRows.Add(row);
                    }
                }

                liveSet = newRows;
            }
        }
        catch (Exception e)
        {
            // Still returns all or nothing as before, but now says why, to whoever is listening (the search bars
            // and mass edit open a SearchErrors capture). It used to look the same as "no matching rows".
            SearchErrors.Report(LOC.Get("PARAM_SE_Error_Condition", currentCondition ?? command, e.Message));

            liveSet = failureAllOrNone ? sourceSet : new List<B>();
        }

        return liveSet;
    }
}

/// <summary>
/// Collects search failures for whoever ran the search. Open a capture around a search, then read
/// <see cref="Capture.FirstError"/>:
/// <code>
/// using var errors = SearchErrors.Begin();
/// var rows = engine.Search(...);
/// var message = errors.FirstError;   // null if every condition applied
/// </code>
/// The failure is reported where it happens, so this also catches failures in nested searches (semijoin, the
/// second stage of a "selection: ... &amp;&amp; ..." search) without changing the Search signature. Captures nest,
/// and each sees the failures reported while it is the innermost one. Per thread.
/// </summary>
public static class SearchErrors
{
    [ThreadStatic]
    private static Capture _current;

    public static Capture Begin()
    {
        var capture = new Capture(_current);
        _current = capture;
        return capture;
    }

    /// <summary>
    /// Records a failure in the current capture (ignored if there is none)
    /// </summary>
    public static void Report(string message)
    {
        _current?.Add(message);
    }

    public sealed class Capture : IDisposable
    {
        private readonly Capture _outer;
        private readonly List<string> _errors = new();
        private bool _disposed;

        internal Capture(Capture outer)
        {
            _outer = outer;
        }

        public IReadOnlyList<string> Errors => _errors;

        /// <summary>
        /// The first failure reported, or null
        /// </summary>
        public string FirstError => _errors.Count > 0 ? _errors[0] : null;

        internal void Add(string message)
        {
            if (!_errors.Contains(message))
            {
                _errors.Add(message);
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_current == this)
            {
                _current = _outer;
            }
        }
    }
}