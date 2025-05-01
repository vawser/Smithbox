using Andre.IO.VFS;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editors.BehaviorEditorNS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialBank
{
    private MaterialData DataParent;

    public string BankName = "Undefined";

    public BankType BankType = BankType.MTD;

    public Dictionary<string, BinderContents> Binders = new();

    private VirtualFileSystem FS;

    public MaterialBank(MaterialData parent, string bankName, VirtualFileSystem targetFs, BankType bankType)
    {
        DataParent = parent;
        BankName = bankName;
        FS = targetFs;
        BankType = bankType;
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
                var binderData = FS.ReadFileOrThrow(filepath);
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
                TaskLogs.AddLog($"[{DataParent.Project.ProjectName}:Material Editor:{BankName}] Failed to load {filepath}", LogLevel.Warning);
            }
        }
    }

    /// <summary>
    /// Save task for this bank
    /// </summary>
    public async Task<bool> Save()
    {
        await Task.Delay(1000);

        var successfulSave = false;

        var selection = DataParent.Project.MaterialEditor.Selection;

        if (!Binders.ContainsKey(selection._selectedFileName))
            return false;

        var binder = Binders[selection._selectedFileName];

        var internalFile = binder.Files.Where(e => e.Name == selection._selectedInternalFileName).FirstOrDefault();

        if (internalFile == null)
            return false;

        using (MemoryStream memoryStream = new MemoryStream(internalFile.Bytes.ToArray()))
        {
            switch (BankType)
            {
                case BankType.MTD:
                    internalFile.Bytes = selection._selectedMaterial.Write();
                    break;
                case BankType.MATBIN:
                    internalFile.Bytes = selection._selectedMatbin.Write();
                    break;
                default: break;
            }
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

public enum BankType
{
    MTD,
    MATBIN
}