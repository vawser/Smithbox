using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;
public class MassEditTemplate
{
    public readonly List<string[]> args;

    public readonly string name;
    public readonly List<string> preamble;
    public readonly string[] text;

    public MassEditTemplate(string path, string name)
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
}