using StudioCore.Core;
using StudioCore.Core.ProjectNS;
using StudioCore.JSON;
using StudioCore.Resources.JSON;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateMeta
{
    public BaseEditor BaseEditor;
    public Project Project;

    public EsdMeta_Root Root;

    public EzStateMeta(BaseEditor editor, Project projectOwner)
    {
        BaseEditor = editor;
        Project = projectOwner;
    }

    public List<EsdMeta_Command> GetAllCommandMeta()
    {
        return Root.commands;
    }

    public EsdMeta_Command GetCommandMeta(long passedBank, long passedId)
    {
        return Root.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
    }

    public List<EsdMeta_Function> GetAllFunctionMeta()
    {
        return Root.functions;
    }

    public EsdMeta_Function GetFunctionMeta(long passedId)
    {
        return Root.functions.Where(e => e.id == passedId).FirstOrDefault();
    }

    public List<EsdMeta_Arg> GetCommandArgMeta(long passedBank, long passedId)
    {
        if (HasCommandArgMeta(passedBank, passedId))
        {
            var command = Root.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
            return command.args;
        }
        else
        {
            return new List<EsdMeta_Arg>();
        }
    }

    public bool HasCommandArgMeta(long passedBank, long passedId)
    {
        var command = Root.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
        if (command != null)
        {
            if (command.args != null && command.args.Count > 0)
            {
                return true;
            }
        }

        return false;
    }

    public List<EsdMeta_Arg> GetFunctionArgMeta(long passedId)
    {
        if (HasFunctionArgMeta(passedId))
        {
            var function = Root.commands.Where(e => e.id == passedId).FirstOrDefault();
            return function.args;
        }
        else
        {
            return new List<EsdMeta_Arg>();
        }
    }

    public bool HasFunctionArgMeta(long passedId)
    {
        var function = Root.commands.Where(e => e.id == passedId).FirstOrDefault();
        if (function != null)
        {
            if (function.args != null && function.args.Count > 0)
            {
                return true;
            }
        }

        return false;
    }
    public EsdMeta_Enum GetArgEnum(long passedBank, long passedId, int argIndex, string argEnumName)
    {
        var command = Root.commands.Where(e => e.bank == passedBank && e.id == passedId).FirstOrDefault();
        if (command != null)
        {
            if (command.args != null && command.args.Count > 0)
            {
                for (int i = 0; i < command.args.Count; i++)
                {
                    var arg = command.args[i];
                    if (i == argIndex)
                    {
                        var argEnumStr = arg.argEnum;

                        var argEnum = Root.enums.Where(e => e.referenceName == argEnumName).FirstOrDefault();
                        if (argEnum != null)
                        {
                            return argEnum;
                        }
                    }
                }
            }
        }

        return null;
    }
}
