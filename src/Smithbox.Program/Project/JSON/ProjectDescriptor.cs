using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectDescriptor
{
    public Guid ProjectGUID;
    public string ProjectName;
    public string ProjectPath;
    public string DataPath;
    public ProjectType ProjectType;

    public bool ImportedParamRowNames;
    public bool AutoSelect;

    public bool EnableMapEditor;
    public bool EnableModelEditor;
    public bool EnableTextEditor;
    public bool EnableParamEditor;
    public bool EnableGparamEditor;
    public bool EnableMaterialEditor;
    public bool EnableTextureViewer;
    public bool EnableFileBrowser;

    public string FolderTag = "";

    public bool EnableExternalMaterialData = true;

    public List<string> PinnedParams { get; set; } = new();
    public Dictionary<string, List<int>> PinnedRows { get; set; } = new();
    public Dictionary<string, List<string>> PinnedFields { get; set; } = new();

    public ProjectDescriptor() { }

    public ProjectDescriptor(ProjectDescriptor newDescriptor)
    {
        ProjectGUID = Guid.NewGuid();
        ProjectName = newDescriptor.ProjectName;
        ProjectPath = newDescriptor.ProjectPath;
        DataPath = newDescriptor.DataPath;
        ProjectType = newDescriptor.ProjectType;

        AutoSelect = newDescriptor.AutoSelect;
        ImportedParamRowNames = newDescriptor.ImportedParamRowNames;

        EnableMapEditor = newDescriptor.EnableMapEditor;
        EnableModelEditor = newDescriptor.EnableModelEditor;
        EnableTextEditor = newDescriptor.EnableTextEditor;
        EnableParamEditor = newDescriptor.EnableParamEditor;
        EnableGparamEditor = newDescriptor.EnableGparamEditor;
        EnableTextureViewer = newDescriptor.EnableTextureViewer;
        EnableMaterialEditor = newDescriptor.EnableMaterialEditor;
        EnableFileBrowser = newDescriptor.EnableFileBrowser;
        EnableExternalMaterialData = newDescriptor.EnableExternalMaterialData;

        FolderTag = newDescriptor.FolderTag;
    }

    public ProjectDescriptor(LegacyProjectDescriptor legacyDescriptor, string projectPath)
    {
        ProjectGUID = Guid.NewGuid();
        ProjectName = legacyDescriptor.ProjectName;
        ProjectPath = Path.GetDirectoryName(projectPath);
        DataPath = legacyDescriptor.GameRoot;
        ProjectType = legacyDescriptor.GameType;

        AutoSelect = true;
        ImportedParamRowNames = false;

        EnableMapEditor = true;
        EnableModelEditor = true;
        EnableTextEditor = true;
        EnableParamEditor = true;
        EnableGparamEditor = true;
        EnableTextureViewer = true;
        EnableMaterialEditor = true;
        EnableFileBrowser = true;

        EnableExternalMaterialData = true;

        FolderTag = "";
    }
}

public class LegacyProjectDescriptor
{
    public string ProjectName { get; set; }
    public string GameRoot { get; set; }
    public ProjectType GameType { get; set; }

    public List<string> PinnedParams { get; set; }
    public Dictionary<string, List<int>> PinnedRows { get; set; }
    public Dictionary<string, List<string>> PinnedFields { get; set; }

    public bool UseLooseParams { get; set; }
    public bool PartialParams { get; set; }
    public string LastFmgLanguageUsed { get; set; }

    public LegacyProjectDescriptor() { }

    public LegacyProjectDescriptor(ProjectEntry curProject)
    {
        ProjectName = curProject.Descriptor.ProjectName;
        GameRoot = curProject.Descriptor.DataPath;
        GameType = curProject.Descriptor.ProjectType;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        UseLooseParams = false;

        // Account for this for DS3 projects
        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.UseLooseParams;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}
