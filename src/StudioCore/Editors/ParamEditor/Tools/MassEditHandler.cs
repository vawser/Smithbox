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
    private ParamEditorScreen Editor;

    public string _currentMEditRegexInput = "";
    public string _lastMEditRegexInput = "";
    public string _mEditRegexResult = "";
    public bool retainMassEditCommand = false;

    public string _newScriptName = "";
    public string _newScriptBody = "";
    public bool _newScriptIsCommon = true;
    public MassEditScript _selectedMassEditScript;

    public MassEditHandler(ParamEditorScreen editor)
    {
        Editor = editor;
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

        Editor._activeView._selection.SortSelection();
        (MassEditResult r, ActionManager child) = MassParamEditRegex.PerformMassEdit(Editor.Project.ParamData.PrimaryBank,
            _currentMEditRegexInput, Editor._activeView._selection);

        if (child != null)
        {
            Editor.EditorActionManager.PushSubManager(child);
        }

        if (r.Type == MassEditResultType.SUCCESS)
        {
            _lastMEditRegexInput = _currentMEditRegexInput;
            _currentMEditRegexInput = "";
            Editor.Project.ParamData.RefreshParamDifferenceCacheTask();
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

        var projectScriptDir = $"{Editor.Project.ProjectPath}\\.smithbox\\Assets\\Scripts\\";
        var scriptPath = $"{projectScriptDir}{_newScriptName}.txt";

        // Check both so the name is unique everywhere
        if (!File.Exists(scriptPath))
        {
            var filename = Path.GetFileNameWithoutExtension(scriptPath);

            try
            {
                var fs = new FileStream(scriptPath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(_newScriptBody);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();

                TaskLogs.AddLog($"Mass Edit: saved mass edit script: {filename} at {scriptPath}.");
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"Mass Edit: to save mass edit script: {filename} at {scriptPath}\n{ex}");
            }
        }
        else
        {
            PlatformUtils.Instance.MessageBox($"{_newScriptName}.txt already exists within the Scripts folder.", "Smithbox", MessageBoxButtons.OK);
        }

        MassEditScript.ReloadScripts(Editor);
    }
}
