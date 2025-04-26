using SoulsFormats;
using StudioCore.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditorNS;

public class CutsceneBank
{
    private CutsceneData DataParent;

    public string BankName = "Undefined";

    public Dictionary<string, MQB> Cutscenes = new();

    public CutsceneBank(CutsceneData parent, string bankName)
    {
        DataParent = parent;
        BankName = bankName;
    }

    public void LoadCutscene(string filename, string filepath)
    {
        var editor = DataParent.Project.MapEditor;
        var cutsceneData = DataParent.Project.FS.ReadFileOrThrow(filepath);

        switch (DataParent.Project.ProjectType)
        {
            case ProjectType.DES:
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                break;
            case ProjectType.DS2:
            case ProjectType.DS2S:
                break;
            case ProjectType.DS3:
                break;
            case ProjectType.BB:
                break;
            case ProjectType.SDT:
                break;
            case ProjectType.ER:
                break;
            case ProjectType.AC6:
                break;
            case ProjectType.ERN:
            default: break;
        }
    }

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
            case ProjectType.AC6:
            case ProjectType.ERN:
            default: break;
        }

        return successfulSave;
    }
}

