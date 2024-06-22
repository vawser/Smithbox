using Andre.Formats;
using ImGuiNET;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Formats;
using StudioCore.Locators;
using StudioCore.Platform;
using StudioCore.Resource;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace StudioCore.Tools
{
    public static class ParamValidationTool
    {
        private static Dictionary<string, PARAMDEF> _paramdefs = new Dictionary<string, PARAMDEF>();
        private static Dictionary<string, Param> _params = new Dictionary<string, Param>();
        private static ulong _paramVersion;

        public static void ValidatePadding(bool allParams = false)
        {
            if (allParams)
            {
                foreach (var entry in ParamBank.VanillaBank.Params)
                {
                    var selectedParamName = entry.Key;
                    ValidatePaddingForParam(selectedParamName);
                }
            }
            else
            {
                var selectedParamName = Smithbox.EditorHandler.ParamEditor._activeView._selection.GetActiveParam();
                ValidatePaddingForParam(selectedParamName);
            }
        }

        public static void ValidatePaddingForParam(string selectedParamName)
        {
            var currentParam = ParamBank.VanillaBank.Params[selectedParamName];
            var currentRow = 0;

            TaskManager.Run(new TaskManager.LiveTask($"Validate {selectedParamName} Padding", TaskManager.RequeueType.None, false,
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
            }));
        }

        public static void ValidateParamdef()
        {
            // Read params from regulation.bin via SF PARAM impl
            _paramdefs = ParamBank._paramdefs;

            var dir = Smithbox.GameRoot;
            var mod = Smithbox.ProjectRoot;

            var param = $@"{mod}\regulation.bin";

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

        private static void LoadParamFromBinder(IBinder parambnd, ref Dictionary<string, Param> paramBank, out ulong version,
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
                            LogLevel.Warning, TaskLogs.LogPriority.Normal, e);
                }

                TaskLogs.AddLog($"{paramName} validated");
            }
        }
    }
}
