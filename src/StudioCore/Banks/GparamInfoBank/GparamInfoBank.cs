using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using System.Text;
using StudioCore.UserProject;
using StudioCore.Banks.AliasBank;

namespace StudioCore.Banks.GparamBank;

/// <summary>
/// An info bank holds information for annotating formats, such as MSB.
/// An info bank has 1 source: Smithbox.
/// </summary>
public class GparamInfoBank
{
    public GparamInfoContainer _loadedInfoBank { get; set; }

    public bool IsLoadingInfoBank { get; set; }
    public bool mayReloadInfoBank { get; set; }

    private string TemplateName = "Template.json";

    private string ProgramDirectory = ".smithbox";

    private string FormatInfoDirectory = "";

    private string FormatInfoName = "";

    public GparamInfoBank()
    {
        mayReloadInfoBank = false;

        FormatInfoName = "GPARAM";
        FormatInfoDirectory = "GPARAM";
    }

    public GparamInfoContainer FormatInformation
    {
        get
        {
            if (IsLoadingInfoBank)
                return null;

            return _loadedInfoBank;
        }
    }

    public void ReloadInfoBank()
    {
        TaskManager.Run(new TaskManager.LiveTask($"Format Info - Load {FormatInfoName}", TaskManager.RequeueType.None, false,
        () =>
        {
            _loadedInfoBank = new GparamInfoContainer();
            IsLoadingInfoBank = true;

            if (Project.Type != ProjectType.Undefined)
            {
                try
                {
                    _loadedInfoBank = new GparamInfoContainer(Project.GameModDirectory);
                }
                catch (Exception e)
                {
                    TaskLogs.AddLog($"FAILED LOAD: {e.Message}");
                }

                IsLoadingInfoBank = false;
            }
            else
                IsLoadingInfoBank = false;
        }));
    }
}
