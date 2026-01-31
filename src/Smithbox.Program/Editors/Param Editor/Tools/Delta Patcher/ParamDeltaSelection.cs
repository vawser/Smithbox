using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class ParamDeltaSelection
{
    private ParamDeltaPatcher Patcher;

    // Importer
    public List<DeltaImportEntry> ImportList = new();
    public DeltaImportEntry SelectedImport = null;
    public bool SelectImportEntry = false;

    public bool QueueImportListRefresh = false;

    // Exporter
    public DeltaExportMode CurrentExportMode = DeltaExportMode.Modified;
    public string ExportName = "";

    public ParamDeltaSelection(ParamDeltaPatcher patcher)
    {
        Patcher = patcher;
    }

    public void RefreshImportList()
    {
        ImportList.Clear();

        var sourceDir = ProjectUtils.GetParamDeltaFolder();

        if (Directory.Exists(sourceDir))
        {
            foreach (var file in Directory.EnumerateFiles(sourceDir))
            {
                var filename = Path.GetFileNameWithoutExtension(file);

                var entry = new DeltaImportEntry();
                entry.Filename = filename;
                entry.Delta = Patcher.LoadDeltaPatch(filename);

                if (CFG.Current.ParamEditor_DeltaPatcher_Import_Display_All_Entries)
                {
                    ImportList.Add(entry);
                }
                else
                {
                    if (Patcher.Project.Descriptor.ProjectType == entry.Delta.ProjectType)
                    {
                        ImportList.Add(entry);
                    }
                }
            }
        }
        else
        {
            Directory.CreateDirectory(sourceDir);
        }
    }

}
