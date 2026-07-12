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
            // Command Palette
            if (ImGui.BeginTabItem($"{LOC.Get("PARAM_MassEdit_Tab_Command_Palette")}##commandPaletteTab"))
            {
                DisplayCommandPalette();

                ImGui.EndTabItem();
            }

            // Templates
            if (ImGui.BeginTabItem($"{LOC.Get("PARAM_MassEdit_Tab_Templates")}##templatesTab"))
            {
                DisplayTemplateMenu();

                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DisplayCommandPalette()
    {
        // Header
        GUI.WrappedText(LOC.Get("PARAM_MassEdit_Command_Palette_TT"));

        // Input
        GUI.Spacer();
        MassEditUtils.MassEditHeader(Parent, 
            LOC.Get("PARAM_MassEdit_Header_Input"),
            LOC.Get("PARAM_MassEdit_Header_Input_TT"));

        GUI.MultilineTextInput("massEditInput", ref Parent.State.CurrentMenuInput);

        GUI.MultiButtonInput("massEditActions", 
            "massEditApply", 
            LOC.Get("PARAM_MassEdit_Action_Apply_Script"),
            LOC.Get("PARAM_MassEdit_Action_Apply_Script_TT"),
            ApplyMassEditAction, 

            "massEditClear", 
            LOC.Get("PARAM_MassEdit_Action_Clear_Script"),
            LOC.Get("PARAM_MassEdit_Action_Clear_Script_TT"),
            ClearMassEditInputAction);

        // Templates
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Templates"),
            LOC.Get("PARAM_MassEdit_Header_Templates_TT"));

        MassEditUtils.TemplateComboBox("massEditScripts", ref CurrentTemplate, ScriptList);

        GUI.MultiButtonInput("massEditScriptActions",
            "massEditScriptLoad", 
            LOC.Get("PARAM_MassEdit_Action_Load_Script"),
            LOC.Get("PARAM_MassEdit_Action_Load_Script_TT"),
            LoadMassEditTemplate);

        // Output
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Output"),
            LOC.Get("PARAM_MassEdit_Header_Output_Command_Palette"));

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
        // Header
        GUI.WrappedText(LOC.Get("PARAM_MassEdit_Template_Hint"));

        // Templates
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Template_Header_Existing"),
            LOC.Get("PARAM_MassEdit_Template_Header_Existing_TT"));

        MassEditUtils.TemplateComboBox("massEditScripts", ref CurrentTemplate, ScriptList);

        GUI.MultiButtonInput("massEditScriptActions",
            "massEditScriptEdit", 
            LOC.Get("PARAM_MassEdit_Action_Edit_Template_Script"),
            LOC.Get("PARAM_MassEdit_Action_Edit_Template_Script_TT"),
            EditMassEditTemplate,

            "massEditScriptReload",
            LOC.Get("PARAM_MassEdit_Action_Update_Template_List"),
            LOC.Get("PARAM_MassEdit_Action_Update_Template_List_TT"),
            ReloadScripts,

            "massEditOpenFolder",
            LOC.Get("PARAM_MassEdit_Action_Open_Template_Folder"),
            LOC.Get("PARAM_MassEdit_Action_Open_Template_Folder_TT"),
            OpenTemplateFolder);

        // Template Contents
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_Template_Contents"),
            LOC.Get("PARAM_MassEdit_Header_Template_Contents_TT"));

        GUI.MultilineTextInput("massEditContents", ref NewScriptContents);

        GUI.MultiButtonInput("massEditContentsActions",
            "massEditScriptSave", 
            LOC.Get("PARAM_MassEdit_Action_Save_Script"),
            LOC.Get("PARAM_MassEdit_Action_Save_Script_TT"),
            SaveMassEditScript,

            "massEditScriptClear", 
            LOC.Get("PARAM_MassEdit_Action_Clear_Script"),
            LOC.Get("PARAM_MassEdit_Action_Clear_Script_TT"),
            ClearMassEditContentsAction);

        // New Template
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_MassEdit_Header_New_Template"),
            LOC.Get("PARAM_MassEdit_Header_New_Template_TT"));

        GUI.SinglelineTextInput("newTemplateName", ref NewScriptName, LOC.Get("PARAM_MassEdit_Input_New_Template"));
        GUI.Tooltip(LOC.Get("PARAM_MassEdit_Input_New_Template_TT"));

        GUI.MultiButtonInput("massEditTemplateActions",
            "massEditScriptSave",
            LOC.Get("PARAM_MassEdit_Action_Save_Script"),
            LOC.Get("PARAM_MassEdit_Action_Save_Script_TT"), 
            SaveMassEditScript);
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
            Smithbox.LogError(this, LOC.Get("PARAM_MassEdit_Log_Template_Name_Empty"));
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

                Smithbox.Log(this, LOC.Get("PARAM_MassEdit_Log_Saved_Template", filename, scriptPath));
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("PARAM_MassEdit_Log_Failed_Save_Template", filename, scriptPath), ex);
            }
        }
        else
        {
            Smithbox.LogError(this, LOC.Get("PARAM_MassEdit_Log_Template_Name_Exists", NewScriptName));
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
                        Smithbox.LogError(this, LOC.Get("PARAM_MassEdit_Log_Failed_Template_Load", name), e);
                        return null;
                    }
                }));
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_MassEdit_Log_Missing_Template_Dir", dir), e);
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
