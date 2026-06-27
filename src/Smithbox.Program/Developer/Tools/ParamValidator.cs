using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Application;

public class ParamValidator
{
    private Dictionary<string, PARAMDEF> _paramdefs = new Dictionary<string, PARAMDEF>();
    private Dictionary<string, Param> _params = new Dictionary<string, Param>();
    private ulong _paramVersion;

    public ParamValidator() { }

    public void Display()
    {
        var project = Smithbox.Orchestrator.SelectedProject;

        if (project == null)
            return;

        if (project.Handler == null)
            return;

        if (project.Handler.ParamEditor == null)
        {
            ImGui.Text("Param Editor must be enabled to use this tool.");
            return;
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("paramActions",
            "validateParamdef",
            "Validate PARAMDEF",
            "Validate that the current PARAMDEF works with the old-style SF PARAM class.",
            ValidateParamdefAction,

            "validatePadding_all",
            "Validate Padding (for all params)",
            "Validate that there are no non-zero values within padding fields.",
            ValidateAllPaddingAction,

            "validatePadding_selected",
            "Validate Padding (for selected param)",
            "Validate that there are no non-zero values within padding fields.",
            ValidateSelectedPaddingAction);
    }

    public void ValidateParamdefAction()
    {
        var project = Smithbox.Orchestrator.SelectedProject;
        ValidateParamdef(project);
    }

    public void ValidateSelectedPaddingAction()
    {
        var project = Smithbox.Orchestrator.SelectedProject;
        ValidatePadding(project);
    }

    public void ValidateAllPaddingAction()
    {
        var project = Smithbox.Orchestrator.SelectedProject;
        ValidatePadding(project, true);
    }

    public void ValidatePadding(ProjectEntry project, bool allParams = false)
    {
        if (allParams)
        {
            foreach (var entry in project.Handler.ParamData.VanillaBank.Params)
            {
                var selectedParamName = entry.Key;
                ValidatePaddingForParam(project, selectedParamName);
            }
        }
        else
        {
            var selectedParamName = project.Handler.ParamEditor.ViewHandler.ActiveView.Selection.GetActiveParam();
            if (selectedParamName != null)
            {
                ValidatePaddingForParam(project, selectedParamName);
            }
        }
    }

    public void ValidatePaddingForParam(ProjectEntry project, string selectedParamName)
    {
        var currentParam = project.Handler.ParamData.VanillaBank.Params[selectedParamName];
        var currentRow = 0;

        TaskManager.LiveTask task = new(
            "system_runParamValidation",
            "[System]",
            "The param validation has run.",
            "The param validation has failed to run.",
            TaskManager.RequeueType.None,
            false,
            () =>
            {
                foreach (var row in currentParam.Rows)
                {
                    currentRow = row.ID;

                    foreach (var cell in row.Cells)
                    {
                        if (cell.Def.InternalType == "dummy8")
                        {
                            //Smithbox.Log(this, cell.Value.GetType().Name);

                            if (cell.Value.GetType() == typeof(byte[]))
                            {
                                // Smithbox.Log(this, $"{currentParam}: {cell.Def.InternalName}");

                                byte[] bytes = (byte[])cell.Value;
                                foreach (var b in bytes)
                                {
                                    if (b != 0)
                                    {
                                        Smithbox.Log(typeof(ParamValidator), $"{selectedParamName}: {currentRow}: {cell.Def.InternalName} contains non-zero values");
                                    }
                                }
                            }
                            else if (cell.Value.GetType() == typeof(byte))
                            {
                                //Smithbox.Log(this, $"{currentParam}: {cell.Def.InternalName}");

                                byte b = (byte)cell.Value;
                                if (b != 0)
                                {
                                    Smithbox.Log(typeof(ParamValidator), $"{selectedParamName}: {currentRow}: {cell.Def.InternalName} contains non-zero values");
                                }
                            }
                        }
                    }
                }
            }
        );

        TaskManager.Run(task);
    }

    public void ValidateParamdef(ProjectEntry curProject)
    {
        // Read params from regulation.bin via SF PARAM impl
        _paramdefs = curProject.Handler.ParamData.ParamDefs;

        var dir = curProject.Descriptor.DataPath;
        var mod = curProject.Descriptor.ProjectPath;

        var param = Path.Join(mod, "regulation.bin");

        // DES, DS1, DS1R
        if (curProject.Descriptor.ProjectType is ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R)
        {
            try
            {
                using BND3 bnd = BND3.Read(param);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {param}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // DS2
        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptDS2Regulation(param);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {param}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // DS3
        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            param = Path.Join(mod, "Data0.bdt");

            try
            {
                using BND4 bnd = SFUtil.DecryptDS3Regulation(param);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {param}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // BB, SDT
        if (curProject.Descriptor.ProjectType is ProjectType.SDT or ProjectType.BB)
        {
            try
            {
                using BND4 bnd = BND4.Read(param);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {param}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // ER
        if (curProject.Descriptor.ProjectType is ProjectType.ER)
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptERRegulation(param);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {param}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        // AC6
        if (curProject.Descriptor.ProjectType is ProjectType.AC6)
        {
            try
            {
                using BND4 bnd = SFUtil.DecryptAC6Regulation(param);
                LoadParamFromBinder(bnd, ref _params, out _paramVersion, true);
            }
            catch (Exception e)
            {
                PlatformUtils.Instance.MessageBox($"Param Load failed: {param}: {e.Message}", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    private void LoadParamFromBinder(IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version,
        bool checkVersion = false, bool validatePadding = false)
    {
        var success = ulong.TryParse(parambnd.Version, out version);
        if (checkVersion && !success)
        {
            throw new Exception(@"Failed to get regulation version. Params might be corrupt.");
        }

        // Load every param in the regulation
        foreach (BinderFile f in parambnd.Files)
        {
            var paramName = Path.GetFileNameWithoutExtension(f.Name);

            if (!f.Name.EndsWith(".PARAM", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            PARAM p;

            p = PARAM.ReadIgnoreCompression(f.Bytes);
            if (!_paramdefs.ContainsKey(p.ParamType ?? ""))
            {
                Smithbox.Log(typeof(ParamValidator), 
                    $"Couldn't find ParamDef for param {paramName} with ParamType \"{p.ParamType}\".",
                    LogLevel.Warning);
                continue;
            }

            if (p.ParamType == null)
            {
                throw new Exception("Param type is unexpectedly null");
            }

            PARAMDEF def = _paramdefs[p.ParamType];
            try
            {
                p.ApplyParamdef(def);
            }
            catch (Exception e)
            {
                var name = f.Name.Split("\\").Last();
                var message = $"Could not apply ParamDef for {name}";

                Smithbox.Log(typeof(ParamValidator), message,
                        LogLevel.Warning, LogPriority.Normal, e);
            }

            Smithbox.Log(typeof(ParamValidator), $"{paramName} validated");
        }
    }
}
