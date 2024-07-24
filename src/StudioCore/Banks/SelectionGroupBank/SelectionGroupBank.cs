using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Localization;
using StudioCore.Locators;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Banks.SelectionGroupBank;

public class SelectionGroupBank
{
    public SelectionGroupList Groups { get; set; }

    private string GroupDirectory = "Workflow\\Entity Selection Groups";

    private string GroupFileName = "Groups.json";

    private string SelectionDirectory = "";
    private string SelectionPath = "";

    public SelectionGroupBank() 
    {
        SelectionDirectory = $"{Smithbox.SmithboxDataRoot}\\{GroupDirectory}\\{MiscLocator.GetGameIDForDir()}\\";
        SelectionPath = $"{SelectionDirectory}\\{GroupFileName}";
    }

    public void LoadBank()
    {
        try
        {
            Groups = BankUtils.LoadSelectionGroupJSON(GroupDirectory, GroupFileName);
        }
        catch (Exception e)
        {
            TaskLogs.AddLog(
                $"{LOC.Get("SELECTION_GROUP_BANK__FAILED_TO_LOAD")}" +
                $"{e.Message}");
        }

        TaskLogs.AddLog($"{LOC.Get("SELECTION_GROUP_BANK__SUCCESSFUL_LOAD")}");
    }

    public void CreateSelectionGroups()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!Directory.Exists(SelectionDirectory))
        {
            try
            {
                Directory.CreateDirectory(SelectionDirectory);
            }
            catch
            {
                TaskLogs.AddLog(
                    $"{LOC.Get("SELECTION_GROUP_BANK__FAILED_TO_MAKE_DIR")}" + 
                    $"{SelectionDirectory}");

                return;
            }

            string template = "{ \"Resources\": [ ] }";
            try
            {
                var fs = new FileStream(SelectionPath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(template);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
    }

    public bool DeleteSelectionGroup(string currentResourceName)
    {
        var resource = Groups.Resources.Where(x => x.Name == currentResourceName).FirstOrDefault();
        Groups.Resources.Remove(resource);

        SaveSelectionGroups();

        return true;
    }

    public bool AddSelectionGroup(string name, List<string> tags, List<string> selection, int keybindIndex, bool isEdit = false, string oldName = "")
    {
        if(Groups.Resources == null)
            Groups.Resources = new List<SelectionGroupResource> { };

        if (name == "")
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("SELECTION_GROUP__GROUP_NAME_IS_EMPTY")}",
                $"{LOC.Get("WARNING")}", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);

            return false;
        }
        else if (!isEdit && Groups.Resources.Any(x => x.Name == name))
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("SELECTION_GROUP__GROUP_NAME_ALREADY_EXISTS")}",
                $"{LOC.Get("WARNING")}", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);

            return false;
        }
        else if (!isEdit && selection == null)
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("SELECTION_GROUP__SELECTION_IS_INVALID")}",
                $"{LOC.Get("WARNING")}", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);

            return false;
        }
        else if (!isEdit && selection.Count == 0)
        {
            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("SELECTION_GROUP__SELECTION_IS_EMPTY")}",
                $"{LOC.Get("WARNING")}", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);

            return false;
        }
        else if (keybindIndex != -1 && Groups.Resources.Any(x => x.SelectionGroupKeybind == keybindIndex))
        {
            var group = Groups.Resources.Where(x => x.SelectionGroupKeybind == keybindIndex).First();
            if (isEdit)
            {
                group = Groups.Resources.Where(x => (x.SelectionGroupKeybind == keybindIndex) && (x.Name != name)).First();
            }

            PlatformUtils.Instance.MessageBox(
                $"{LOC.Get("SELECTION_GROUP__KEYBIND_ALREADY_ASSIGNED")}" +
                $"{group.Name}",
                $"{LOC.Get("WARNING")}", 
                MessageBoxButtons.OK, 
                MessageBoxIcon.Error);

            return false;
        }
        else
        {
            // Delete old entry, since we will create it a-new with the edits immediately
            if (isEdit)
            {
                DeleteSelectionGroup(oldName);
            }

            var res = new SelectionGroupResource();
            res.Name = name;
            res.Tags = tags;
            res.Selection = selection;
            res.SelectionGroupKeybind = keybindIndex;

            Groups.Resources.Add(res);

            SaveSelectionGroups();
        }

        return false;
    }

    public bool SaveSelectionGroups()
    {
        if (Smithbox.ProjectType == ProjectType.Undefined)
            return false;

        string jsonString = JsonSerializer.Serialize(Groups, typeof(SelectionGroupList), SelectionGroupListSerializationContext.Default);

        if (Directory.Exists(SelectionDirectory))
        {
            try
            {
                var fs = new FileStream(SelectionPath, System.IO.FileMode.Create);
                var data = Encoding.ASCII.GetBytes(jsonString);
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                TaskLogs.AddLog($"{ex}");
            }
        }
        else
        {
            return false;
        }

        return true;
    }
}
