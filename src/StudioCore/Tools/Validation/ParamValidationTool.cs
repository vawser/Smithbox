using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Formats;
using StudioCore.Platform;
using StudioCore.Resource;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StudioCore.Tools.Validation;

public class ParamValidationTool
{
    private Smithbox BaseEditor;

    public ParamValidationTool(Smithbox editor)
    {
        BaseEditor = editor;
    }

    private Dictionary<string, PARAMDEF> _paramdefs = new Dictionary<string, PARAMDEF>();
    private Dictionary<string, Param> _params = new Dictionary<string, Param>();
    private ulong _paramVersion;

    public void ValidatePadding(bool allParams = false)
    {
        var curProject = BaseEditor.ProjectManager.SelectedProject;

        if (allParams)
        {
            foreach (var entry in curProject.ParamData.VanillaBank.Params)
            {
                var selectedParamName = entry.Key;
                ValidatePaddingForParam(selectedParamName);
            }
        }
        else
        {
            var selectedParamName = curProject.ParamEditor._activeView._selection.GetActiveParam();
            if (selectedParamName != null)
            {
                ValidatePaddingForParam(selectedParamName);
            }
        }
    }

    public void ValidatePaddingForParam(string selectedParamName)
    {
        var curProject = BaseEditor.ProjectManager.SelectedProject;

        var currentParam = curProject.ParamData.VanillaBank.Params[selectedParamName];
        var currentRow = 0;

        TaskManager.LiveTask task = new(
            "system_runParamValidation",
            "System",
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
                            //TaskLogs.AddLog(cell.Value.GetType().Name);

                            if (cell.Value.GetType() == typeof(byte[]))
                            {
                                // TaskLogs.AddLog($"{currentParam}: {cell.Def.InternalName}");

                                byte[] bytes = (byte[])cell.Value;
                                foreach (var b in bytes)
                                {
                                    if (b != 0)
                                    {
                                        TaskLogs.AddLog($"{selectedParamName}: {currentRow}: {cell.Def.InternalName} contains non-zero values");
                                    }
                                }
                            }
                            else if (cell.Value.GetType() == typeof(byte))
                            {
                                //TaskLogs.AddLog($"{currentParam}: {cell.Def.InternalName}");

                                byte b = (byte)cell.Value;
                                if (b != 0)
                                {
                                    TaskLogs.AddLog($"{selectedParamName}: {currentRow}: {cell.Def.InternalName} contains non-zero values");
                                }
                            }
                        }
                    }
                }
            }
        );

        TaskManager.Run(task);
    }

    public void ValidateParamdef()
    {
        var curProject = BaseEditor.ProjectManager.SelectedProject;

        // Read params from regulation.bin via SF PARAM impl
        _paramdefs = curProject.ParamData.ParamDefs;

        var dir = curProject.DataPath;
        var mod = curProject.ProjectPath;

        var param = $@"{mod}\regulation.bin";

        // DES, DS1, DS1R
        if (curProject.ProjectType == ProjectType.DES || curProject.ProjectType == ProjectType.DS1 || curProject.ProjectType == ProjectType.DS1R)
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
        if (curProject.ProjectType == ProjectType.DS2 || curProject.ProjectType == ProjectType.DS2S)
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
        if (curProject.ProjectType == ProjectType.DS3)
        {
            param = $@"{mod}\Data0.bdt";

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
        if (curProject.ProjectType == ProjectType.SDT || curProject.ProjectType == ProjectType.BB)
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
        if (curProject.ProjectType == ProjectType.ER)
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
        if (curProject.ProjectType == ProjectType.AC6)
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

            if (!f.Name.ToUpper().EndsWith(".PARAM"))
            {
                continue;
            }

            PARAM p;

            p = PARAM.ReadIgnoreCompression(f.Bytes);
            if (!_paramdefs.ContainsKey(p.ParamType ?? ""))
            {
                TaskLogs.AddLog(
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

                TaskLogs.AddLog(message,
                        LogLevel.Warning, LogPriority.Normal, e);
            }

            TaskLogs.AddLog($"{paramName} validated");
        }
    }
}
