using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Resource.Locators;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.ParamEditor.MassEdit;

public class MassEditScript
{
    public static List<MassEditScript> scriptList = new List<MassEditScript>();
    private readonly List<string[]> args;

    public readonly string name;
    public readonly List<string> preamble;
    public readonly string[] text;

    private MassEditScript(string path, string name)
    {
        List<string> preamble = new();
        var text = File.ReadAllLines(path);
        List<string[]> args = new();
        foreach (var line in text)
        {
            if (line.StartsWith("##") && args.Count == 0)
            {
                preamble.Add(line);
            }
            else if (line.StartsWith("newvar "))
            {
                var arg = line.Substring(7).Split(':', 2);
                if (arg[1].EndsWith(';'))
                {
                    arg[1] = arg[1].Substring(0, arg[1].Length - 1);
                }

                args.Add(arg);
            }
            else
            {
                break;
            }
        }

        this.name = name;
        this.preamble = preamble;
        this.text = text;
        this.args = args;
    }

    public static void ReloadScripts(ParamEditorScreen editor)
    {
        if (editor.Project.ProjectType is ProjectType.Undefined)
            return;

        var cdir = ParamLocator.GetMassEditScriptCommonDir();
        var dir = ParamLocator.GetMassEditScriptGameDir(editor.Project);
        scriptList = new List<MassEditScript>();
        LoadScriptsFromDir(cdir);
        LoadScriptsFromDir(dir);

        var projectScriptDir = $"{editor.Project.ProjectPath}\\.smithbox\\Assets\\Scripts\\";

        if (Directory.Exists(projectScriptDir))
        {
            LoadScriptsFromDir(projectScriptDir, true);
        }
        else
        {
            Directory.CreateDirectory(projectScriptDir);
        }
    }

    private static void LoadScriptsFromDir(string dir, bool isProjectScript = false)
    {
        try
        {
            if (Directory.Exists(dir))
            {
                scriptList.AddRange(Directory.GetFiles(dir).Select(x =>
                {
                    var name = x;
                    try
                    {
                        name = Path.GetFileNameWithoutExtension(x);
                        if (isProjectScript)
                            name = $"Project: {name}";

                        return new MassEditScript(x, name);
                    }
                    catch (Exception e)
                    {
                        TaskLogs.AddLog($"Error loading mass edit script {name}",
                            LogLevel.Warning, LogPriority.Normal, e);
                        return null;
                    }
                }));
            }
        }
        catch (Exception e)
        {
            TaskLogs.AddLog($"Error loading mass edit scripts in {dir}",
                LogLevel.Warning, LogPriority.Normal, e);
        }
    }

    public static void EditorScreenMenuItems(ref string _currentMEditRegexInput)
    {
        foreach (MassEditScript script in scriptList)
        {
            if (script == null)
            {
                continue;
            }

            if (ImGui.BeginMenu(script.name))
            {
                script.MenuItems();
                if (ImGui.Selectable("Load"))
                {
                    _currentMEditRegexInput = script.GenerateMassedit();
                    EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
                }

                ImGui.EndMenu();
            }
        }
    }

    public void MenuItems()
    {
        foreach (var s in preamble)
        {
            ImGui.TextUnformatted(s.Substring(2));
        }

        ImGui.Separator();
        foreach (var arg in args)
        {
            ImGui.InputText(arg[0], ref arg[1], 128);
        }
    }

    public string GenerateMassedit()
    {
        var addedCommands = preamble.Count == 0 ? "" : "\n" + "clear;\nclearvars;\n";
        return string.Join('\n', preamble) + addedCommands +
               string.Join('\n', args.Select(x => $@"newvar {x[0]}:{x[1]};")) + '\n' +
               string.Join('\n', text.Skip(args.Count + preamble.Count));
    }
}
