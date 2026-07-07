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

        // Tabs
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

        // Header
        GUI.WrappedText("Write and execute mass edit commands here.");

        // Input
        GUI.WrappedText("");
        MassEditUtils.MassEditHeader(Parent, 
            "Input", "Type in the mass edit commands you wish to apply here.");

        GUI.MultilineTextInput("massEditInput", ref Parent.State.CurrentMenuInput);

        GUI.MultiButtonInput("massEditActions", 
            "massEditApply", "Apply", "Apply this script", ApplyMassEditAction, 
            "massEditClear", "Clear", "Clear this script", ClearMassEditInputAction);

        // Templates
        GUI.WrappedText("");
        GUI.SimpleHeader("Templates", "");

        MassEditUtils.TemplateComboBox("massEditScripts",ref CurrentTemplate, ScriptList);

        GUI.MultiButtonInput("massEditScriptActions",
            "massEditScriptLoad", "Load", "Load this script", LoadMassEditTemplate);

        // Output
        GUI.WrappedText("");
        GUI.SimpleHeader("Output", "Success state of the Mass Edit command that was previously used.\n\nRemember to handle clipboard state between edits with the 'clear' command");
        GUI.WrappedText($"{Parent.State.MassEditResult}");
    }

    public void ApplyMassEditAction()
    {
        Parent.ExecuteMassEdit(
            Parent.State.CurrentMenuInput,
            Parent.CurrentView.GetPrimaryBank(),
            Parent.CurrentView.Selection);
    }

    public void ClearMassEditInputAction()
    {
        Parent.State.CurrentMenuInput = "";
    }

    private void DisplayTemplateMenu()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var inputBoxSize = new Vector2(windowWidth * 0.725f, 32);

        // Header
        GUI.WrappedText("Create and edit mass edit templates here.");

        // Templates
        GUI.WrappedText("");
        GUI.SimpleHeader("Existing Templates", "");

        MassEditUtils.TemplateComboBox("massEditScripts", ref CurrentTemplate, ScriptList);

        GUI.MultiButtonInput("massEditScriptActions",
            "massEditScriptEdit", "Edit", "Edit this script", EditMassEditTemplate,
            "massEditScriptReload", "Update Template List", "", ReloadScripts,
            "massEditOpenFolder", "Open Template Folder", "", OpenTemplateFolder);

        // Template Contents
        GUI.WrappedText("");
        GUI.SimpleHeader("Template Contents", "");

        GUI.MultilineTextInput("massEditContents", ref NewScriptContents);

        GUI.MultiButtonInput("massEditContentsActions",
            "massEditScriptSave", "Save", "Save this script", SaveMassEditScript,
            "massEditScriptClear", "Clear", "Clear this script", ClearMassEditContentsAction);

        // New Template
        GUI.WrappedText("");
        GUI.SimpleHeader("New Template", "");

        GUI.SinglelineTextInput("newTemplateName",ref NewScriptName, "Name");
        GUI.Tooltip("The file name used for this template.");

        GUI.MultiButtonInput("massEditTemplateActions",
            "massEditScriptSave", "Save", "Save this script", SaveMassEditScript);
    }

    public void LoadMassEditTemplate()
    {
        if (CurrentTemplate != null)
        {
            Parent.State.CurrentMenuInput = GenerateMassedit(CurrentTemplate);
        }
    }

    public void EditMassEditTemplate()
    {
        NewScriptName = CurrentTemplate.name;
        NewScriptContents = GenerateMassedit(CurrentTemplate);
    }

    public void ClearMassEditContentsAction()
    {
        NewScriptName = "";
        NewScriptContents = "";
    }

    public void OpenTemplateFolder()
    {
        var projectScriptDir = Path.Combine(Parent.Editor.Project.Descriptor.ProjectPath, ".smithbox", "Assets", "Scripts");

        Process.Start("explorer.exe", projectScriptDir);
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
