using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Actions;
using StudioCore.Platform;
using StudioCore.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public class MassEditHandler
{
    private ParamEditorScreen Screen;

    public string _currentMEditRegexInput = "";
    public string _lastMEditRegexInput = "";
    public string _mEditRegexResult = "";
    public bool retainMassEditCommand = false;

    public string _newScriptName = "";
    public string _newScriptBody = "";
    public bool _newScriptIsCommon = true;
    public MassEditScript _selectedMassEditScript;

    public MassEditHandler(ParamEditorScreen screen)
    {
        Screen = screen;
    }

    public void Shortcuts()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_ExecuteMassEdit))
        {
            ExecuteMassEdit();
        }
    }

    public void ExecuteMassEdit()
    {
        var command = _currentMEditRegexInput;

        Smithbox.EditorHandler.ParamEditor._activeView._selection.SortSelection();
        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
            _currentMEditRegexInput, Smithbox.EditorHandler.ParamEditor._activeView._selection);

        if (child != null)
        {
            Screen.EditorActionManager.PushSubManager(child);
        }

        if (r.Type == MassEditResultType.SUCCESS)
        {
            _lastMEditRegexInput = _currentMEditRegexInput;
            _currentMEditRegexInput = "";
            TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
                TaskManager.RequeueType.Repeat,
                true, LogPriority.Low,
                () => ParamBank.RefreshAllParamDiffCaches(false)));
        }

        _mEditRegexResult = r.Information;

        if (retainMassEditCommand)
        {
            _currentMEditRegexInput = command;
        }
    }

    public void MassEditScriptSetup()
    {
        if (MassEditScript.scriptList.Count > 0)
        {
            if (_selectedMassEditScript == null)
            {
                _selectedMassEditScript = MassEditScript.scriptList[0];
            }
        }
    }

    public void SaveMassEditScript()
    {
        if (_newScriptName == "")
        {
            PlatformUtils.Instance.MessageBox($"Name must not be empty.", "Smithbox", MessageBoxButtons.OK);
            return;
        }

        var projectScriptDir = $"{Smithbox.ProjectRoot}\\.smithbox\\Assets\\MassEditScripts\\";
        var scriptPath = $"{projectScriptDir}{_newScriptName}.txt";

        // Check both so the name is unique everywhere
        if (!File.Exists(scriptPath))
        {
            try
            {
                var fs = new FileStream(scriptPath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(_newScriptBody);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox($"{_newScriptName}.txt already exists within the MassEditScripts folder.", "Smithbox", MessageBoxButtons.OK);
        }

        MassEditScript.ReloadScripts();
    }
}
