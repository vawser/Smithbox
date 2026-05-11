using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class MassEditToolMenu
{
    public MassEdit Parent;

    public bool InitialScriptLoad = false;

    public List<MassEditTemplate> ScriptList = new List<MassEditTemplate>();
    public MassEditTemplate CurrentTemplate;

    public string NewScriptName = "";
    public string NewScriptContents = "";
    public bool IsScriptCommon = true;

    public MassEditToolMenu(MassEdit parent)
    {
        Parent = parent;
    }

    public void Display()
    {
        MassEditScriptSetup();

        if (ImGui.BeginTabBar("massEditTabs"))
        {
            if (ImGui.BeginTabItem("Command Palette"))
            {
                DisplayCommandPalette();

                ImGui.EndTabItem();
            }


            if (ImGui.BeginTabItem("Templates"))
            {
                DisplayTemplateMenu();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DisplayCommandPalette()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2(windowWidth * 0.725f, 32);

        var Size = ImGui.GetWindowSize();
        float EditX = Size.X * 0.92f;
        float EditY = Size.Y * 0.1f;

        UIHelper.WrappedText("Write and execute mass edit commands here.");
        UIHelper.WrappedText("");

        // AutoFill
        if (Parent.AutoFill != null)
        {
            var res = Parent.AutoFill.MassEditCompleteAutoFill();
            if (res != null)
            {
                Parent.State.CurrentMenuInput = Parent.State.CurrentMenuInput + res;
            }
        }

        // Input
        UIHelper.SimpleHeader("Input", "Type in the mass edit commands you wish to apply here.");

        ImGui.InputTextMultiline("##MEditRegexInput", ref Parent.State.CurrentMenuInput, 65536,
        new Vector2(EditX, EditY));

        if (ImGui.Button("Apply##action_Selection_MassEdit_Execute", DPI.StandardButtonSize))
        {
            Parent.ExecuteMassEdit(
                Parent.State.CurrentMenuInput,
                Parent.CurrentView.GetPrimaryBank(),
                Parent.CurrentView.Selection);
        }

        ImGui.SameLine();

        if (ImGui.Button("Clear##action_Selection_MassEdit_Clear", DPI.StandardButtonSize))
        {
            Parent.State.CurrentMenuInput = "";
        }

        ImGui.Text("");

        // Templates
        UIHelper.SimpleHeader("Templates", "");

        if (ImGui.BeginCombo("##massEditScripts", CurrentTemplate.name))
        {
            foreach (var script in ScriptList)
            {
                if (ImGui.Selectable(script.name, CurrentTemplate.name == script.name))
                {
                    CurrentTemplate = script;
                }
            }

            ImGui.EndCombo();
        }

        if (CurrentTemplate != null)
        {
            ImGui.SameLine();
            if (ImGui.Button("Load"))
            {
                Parent.State.CurrentMenuInput = GenerateMassedit(CurrentTemplate);
            }
        }

        UIHelper.WrappedText("");

        // Output
        UIHelper.SimpleHeader("Output", "Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");
        UIHelper.WrappedText($"{Parent.State.MassEditResult}");
    }

    private void DisplayTemplateMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2(windowWidth * 0.725f, 32);

        UIHelper.WrappedText("Create and edit mass edit templates here.");
        UIHelper.WrappedText("");

        UIHelper.SimpleHeader("Existing Templates", "");

        // Scripts
        if (ImGui.BeginCombo("##massEditScripts", CurrentTemplate.name))
        {
            foreach (var script in ScriptList)
            {
                if (ImGui.Selectable(script.name, CurrentTemplate.name == script.name))
                {
                    CurrentTemplate = script;
                }
            }

            ImGui.EndCombo();
        }

        if (CurrentTemplate != null)
        {
            if (ImGui.Button("Load"))
            {
                Parent.State.CurrentMenuInput = GenerateMassedit(CurrentTemplate);
            }

            ImGui.SameLine();
            if (ImGui.Button("Edit"))
            {
                NewScriptName = CurrentTemplate.name;
                NewScriptContents = GenerateMassedit(CurrentTemplate);
            }
            ImGui.SameLine();
        }

        if (ImGui.Button("Reload"))
        {
            ReloadScripts();
        }

        UIHelper.WrappedText("");

        UIHelper.SimpleHeader("New Template", "");

        ImGui.InputText("##scriptName", ref NewScriptName, 255);
        UIHelper.Tooltip("The file name used for this template.");
        UIHelper.WrappedText("");

        var Size = ImGui.GetWindowSize();
        float EditX = Size.X / 100 * 975;
        float EditY = Size.Y / 100 * 10;

        UIHelper.WrappedText("Template:");
        UIHelper.Tooltip("The mass edit template.");

        ImGui.InputTextMultiline("##newMassEditScript", ref NewScriptContents, 65536, new Vector2(EditX, EditY));

        UIHelper.WrappedText("");

        if (ImGui.Button("Save"))
        {
            SaveMassEditScript();
        }

        ImGui.SameLine();

        if (ImGui.Button("Open Template Folder"))
        {
            var projectScriptDir = Path.Combine(Parent.Editor.Project.Descriptor.ProjectPath, ".smithbox", "Assets", "Scripts");

            Process.Start("explorer.exe", projectScriptDir);
        }
    }
    private void MassEditScriptSetup()
    {
        if (!InitialScriptLoad)
        {
            ReloadScripts();

            InitialScriptLoad = true;
        }

        if (ScriptList.Count > 0)
        {
            if (CurrentTemplate == null)
            {
                CurrentTemplate = ScriptList[0];
            }
        }
    }

    public void SaveMassEditScript()
    {
        if (NewScriptName == "")
        {
            Smithbox.LogError(this, "Mass Edit Template name must not be empty.");
            return;
        }

        var scriptPath = Path.Combine(Parent.Editor.Project.Descriptor.ProjectPath, ".smithbox", "Assets", "Scripts", $"{NewScriptName}.txt");

        // Check both so the name is unique everywhere
        if (!File.Exists(scriptPath))
        {
            var filename = Path.GetFileNameWithoutExtension(scriptPath);

            try
            {
                var fs = new FileStream(scriptPath, FileMode.Create);
                var data = Encoding.ASCII.GetBytes(NewScriptContents);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();

                Smithbox.Log(this, $"Mass Edit: saved mass edit template: {filename} at {scriptPath}.");
            }
            catch (Exception ex)
            {
                Smithbox.Log(this, $"Mass Edit: to save mass edit template: {filename} at {scriptPath}\n{ex}");
            }
        }
        else
        {
            Smithbox.LogError(this, $"Mass Edit template with this name already exists: {NewScriptName}.");
        }

        ReloadScripts();
    }


    public void ReloadScripts()
    {
        var cdir = Path.Join("Assets", "Scripts", "Common");
        var dir = Path.Join("Assets", "Scripts", ProjectUtils.GetGameDirectory(Parent.Project));

        ScriptList = new List<MassEditTemplate>();

        LoadScriptsFromDir(cdir);
        LoadScriptsFromDir(dir);

        var projectScriptDir = Path.Combine(Parent.Project.Descriptor.ProjectPath, ".smithbox", "Assets", "Scripts");

        if (Directory.Exists(projectScriptDir))
        {
            LoadScriptsFromDir(projectScriptDir, true);
        }
        else
        {
            Directory.CreateDirectory(projectScriptDir);
        }
    }

    private void LoadScriptsFromDir(string dir, bool isProjectScript = false)
    {
        try
        {
            if (Directory.Exists(dir))
            {
                ScriptList.AddRange(Directory.GetFiles(dir).Select(x =>
                {
                    var name = x;
                    try
                    {
                        name = Path.GetFileNameWithoutExtension(x);
                        if (isProjectScript)
                            name = $"Project: {name}";

                        return new MassEditTemplate(x, name);
                    }
                    catch (Exception e)
                    {
                        Smithbox.Log(this, $"Error loading mass edit script {name}",
                            LogLevel.Warning, LogPriority.Normal, e);
                        return null;
                    }
                }));
            }
        }
        catch (Exception e)
        {
            Smithbox.Log(this, $"Error loading mass edit scripts in {dir}",
                LogLevel.Warning, LogPriority.Normal, e);
        }
    }

    public void EditorScreenMenuItems(ref string _currentMEditRegexInput)
    {
        foreach (var script in ScriptList)
        {
            if (script == null)
            {
                continue;
            }

            if (ImGui.BeginMenu(script.name))
            {
                MenuItems(script);

                if (ImGui.Selectable("Load"))
                {
                    _currentMEditRegexInput = GenerateMassedit(script);
                    EditorCommandQueue.AddCommand(@"param/menu/massEditRegex");
                }

                ImGui.EndMenu();
            }
        }
    }

    public void MenuItems(MassEditTemplate template)
    {
        foreach (var s in template.preamble)
        {
            ImGui.TextUnformatted(s.Substring(2));
        }

        ImGui.Separator();
        foreach (var arg in template.args)
        {
            ImGui.InputText(arg[0], ref arg[1], 128);
        }
    }

    public string GenerateMassedit(MassEditTemplate template)
    {
        var addedCommands = template.preamble.Count == 0 ? "" : "\n" + "clear;\nclearvars;\n";
        return string.Join('\n', template.preamble) + addedCommands +
               string.Join('\n', template.args.Select(x => $@"newvar {x[0]}:{x[1]};")) + '\n' +
               string.Join('\n', template.text.Skip(template.args.Count + template.preamble.Count));
    }
}
