using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.BehaviorEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.BehaviorEditorNS;

public class BehaviorBank
{
    private BehaviorData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, BinderContents> Binders = new();

    public BehaviorBank(BehaviorData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    /// <summary>
    /// Load the external file
    /// </summary>
    public void LoadBinder(string filename, string filepath)
    {
        // Read binder if it hasn't already been loaded
        if (!Binders.ContainsKey(filename))
        {
            try
            {
                var binderData = DataParent.Project.FS.ReadFileOrThrow(filepath);
                var curBinder = BND4.Read(binderData);

                var newBinderContents = new BinderContents();
                newBinderContents.Name = filename;

                var fileList = new List<BinderFile>();
                foreach (var file in curBinder.Files)
                {
                    fileList.Add(file);
                }

                newBinderContents.Binder = curBinder;
                newBinderContents.Files = fileList;

                Binders.Add(filename, newBinderContents);
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Behavior Editor:{BankName}] Failed to load {filepath}", LogLevel.Warning);
            }
        }
    }

    private string SearchFolder = @"Behaviors\";

    /// <summary>
    /// Load the internal file
    /// </summary>
    public void LoadInternalFile()
    {
        var selection = DataParent.Project.BehaviorEditor.Selection;

        if (!Binders.ContainsKey(selection._selectedFileName))
            return;

        var binder = Binders[selection._selectedFileName];

        var internalFile = binder.Files.Where(e => e.Name == selection._selectedInternalFileName).FirstOrDefault();

        if (internalFile == null)
            return;

        selection.SelectHavokRoot(internalFile);
    }

    /// <summary>
    /// Save task for this bank
    /// </summary>
    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
            case ProjectType.DS1:
            case ProjectType.DS1R:
            case ProjectType.DS2:
            case ProjectType.DS2S:
            case ProjectType.DS3:
            case ProjectType.BB:
            case ProjectType.SDT:
            case ProjectType.ER:
                return SaveBehavior_ER();
            case ProjectType.AC6:
            case ProjectType.ERN:
            default: break;
        }

        return successfulSave;
    }

    public bool SaveBehavior_ER()
    {
        var selection = DataParent.Project.BehaviorEditor.Selection;

        if (!Binders.ContainsKey(selection._selectedFileName))
            return false;

        var binder = Binders[selection._selectedFileName];

        var internalFile = binder.Files.Where(e => e.Name == selection._selectedInternalFileName).FirstOrDefault();

        if (internalFile == null)
            return false;

        using (MemoryStream memoryStream = new MemoryStream(internalFile.Bytes.ToArray()))
        {
            selection._selectedSerializer.Write(selection._selectedHavokRoot, memoryStream);
            internalFile.Bytes = memoryStream.ToArray();
        }

        return true;
    }
}

public class BinderContents
{
    public string Name { get; set; }
    public BND4 Binder { get; set; }
    public List<BinderFile> Files { get; set; }
}