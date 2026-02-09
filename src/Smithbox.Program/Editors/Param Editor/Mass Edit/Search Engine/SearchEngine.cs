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
        filterList.Add("exists", newCmd(new string[0], "Selects all elements", noArgs(noContext(B => true))));
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
                options.Add(op + "(" + filterList[op].args.Length + " args)");
            }
        }

        if (defaultFilter != null && (defaultFilter.shouldShow == null || defaultFilter.shouldShow()))
        {
            options.Add("or omit specifying and use default (" + defaultFilter.args.Length + "args)");
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

        try
        {
            foreach (var condition in conditions)
            {
                //temp
                if (condition.Equals(""))
                {
                    break;
                }

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

                for (var i = 0; i < argC; i++)
                {
                    if (args[i].StartsWith('$'))
                    {
                        args[i] = MassParamEdit.massEditVars[args[i].Substring(1)].ToString();
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
        catch (Exception)
        {
            //Smithbox.LogError(this, $"[Smithbox:Param Editor] Search Engine search failed.", LogPriority.High, e);

            liveSet = failureAllOrNone ? sourceSet : new List<B>();
        }

        return liveSet;
    }
}