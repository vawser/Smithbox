using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Application;

public class ProjectDescriptor
{
    public Guid ProjectGUID = Guid.Empty;
    public string ProjectName = "";
    public string ProjectPath = "";
    public string DataPath = "";
    public ProjectType ProjectType = ProjectType.DS1;

    public bool ImportedParamRowNames = false;
    public bool AutoSelect = false;

    public string FolderTag = "";

    public bool EnableExternalMaterialData = true;

    public bool EnableMapEditor = false;
    public bool EnableModelEditor = false;
    public bool EnableTextEditor = false;
    public bool EnableParamEditor = false;
    public bool EnableGparamEditor = false;
    public bool EnableMaterialEditor = false;
    public bool EnableTextureViewer = false;
    public bool EnableFileBrowser = false;

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
    public ProjectDescriptor(LegacyProjectDescriptorAlt legacyDescriptor, string projectPath)
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
        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS2;
        }

        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS3;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}


public class LegacyProjectDescriptorAlt
{
    public string ProjectName { get; set; }
    public string GameRoot { get; set; }
    public ProjectType GameType { get; set; }

    public List<string> PinnedParams { get; set; }
    public List<int> PinnedRows { get; set; }
    public List<string> PinnedFields { get; set; }

    public bool UseLooseParams { get; set; }
    public bool PartialParams { get; set; }
    public string LastFmgLanguageUsed { get; set; }

    public LegacyProjectDescriptorAlt() { }

    public LegacyProjectDescriptorAlt(ProjectEntry curProject)
    {
        ProjectName = curProject.Descriptor.ProjectName;
        GameRoot = curProject.Descriptor.DataPath;
        GameType = curProject.Descriptor.ProjectType;

        PinnedParams = new();
        PinnedRows = new();
        PinnedFields = new();

        UseLooseParams = false;

        // Account for this for DS3 projects
        if (curProject.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS2;
        }

        if (curProject.Descriptor.ProjectType is ProjectType.DS3)
        {
            UseLooseParams = CFG.Current.ParamEditor_Loose_Param_Mode_DS3;
        }

        PartialParams = false;

        LastFmgLanguageUsed = "engus";
    }
}