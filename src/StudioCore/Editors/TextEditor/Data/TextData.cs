using StudioCore.Core;
using StudioCore.Editors.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Data;

public class TextData
{
    public Smithbox BaseEditor;
    public ProjectEntry Project;

    public TextBank PrimaryBank;
    public TextBank VanillaBank;
    public TextBank AuxBank;

    public TextData(Smithbox baseEditor, ProjectEntry project)
    {
        BaseEditor = baseEditor;
        Project = project;
    }

    public async Task<bool> Setup()
    {
        await Task.Delay(1);

        PrimaryBank = new(BaseEditor, Project, Project.ProjectPath, Project.DataPath);
        VanillaBank = new(BaseEditor, Project, Project.DataPath, Project.DataPath);

        // Primary Bank
        Task<bool> primaryBankTask = PrimaryBank.Setup();
        bool primaryBankTaskResult = await primaryBankTask;

        if (primaryBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Setup Primary FMG Bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to setup Primary FMG Bank.");
        }

        // Vanilla Bank
        Task<bool> vanillaBankTask = VanillaBank.Setup();
        bool vanillaBankTaskResult = await vanillaBankTask;

        if (vanillaBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Setup Vanilla FMG Bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to setup Vanilla FMG Bank.");
        }

        return true;
    }

    public async void LoadAuxBank(string sourcePath)
    {
        AuxBank = new(BaseEditor, Project, sourcePath, Project.DataPath);

        // Aux Bank
        Task<bool> auxBankTask = AuxBank.Setup();
        bool auxBankTaskResult = await auxBankTask;

        if (auxBankTaskResult)
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Setup Aux FMG Bank.");
        }
        else
        {
            TaskLogs.AddLog($"[{Project.ProjectName}:Text Editor] Failed to setup Aux FMG Bank.");
        }
    }
}
