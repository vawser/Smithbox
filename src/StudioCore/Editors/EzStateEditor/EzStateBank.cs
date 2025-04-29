using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.EventScriptEditorNS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EzStateEditorNS;

public class EzStateBank
{
    private EzStateData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, BinderContents> Binders = new();

    public EzStateBank(EzStateData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:EzState Editor:{BankName}] Failed to load {filepath}", LogLevel.Warning);
            }
        }
    }

    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        var selection = DataParent.Project.EzStateEditor.Selection;

        if (!Binders.ContainsKey(selection.SelectedFilename))
            return false;

        var binder = Binders[selection.SelectedFilename];

        var internalFile = binder.Files.Where(e => e.Name == selection.SelectedInternalFilename).FirstOrDefault();

        if (internalFile == null)
            return false;

        using (MemoryStream memoryStream = new MemoryStream(internalFile.Bytes.ToArray()))
        {
            selection.SelectedScript.Write();
            internalFile.Bytes = memoryStream.ToArray();
        }

        return successfulSave;
    }
}

public class BinderContents
{
    public string Name { get; set; }
    public BND4 Binder { get; set; }
    public List<BinderFile> Files { get; set; }
}