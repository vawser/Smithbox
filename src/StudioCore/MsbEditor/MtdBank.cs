using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.MsbEditor;

public class MtdBank
{
    private static AssetLocator AssetLocator;

    private static Dictionary<string, MTD> _mtds = new();
    private static Dictionary<string, MATBIN> _matbins = new();

    public static bool IsMatbin { get; private set; }

    public static IReadOnlyDictionary<string, MTD> Mtds => _mtds;

    public static IReadOnlyDictionary<string, MATBIN> Matbins => _matbins;

    public static void ReloadMtds()
    {
        TaskManager.Run(new TaskManager.LiveTask("Resource - Load MTDs", TaskManager.RequeueType.WaitThenRequeue,
            false, () =>
            {
                try
                {
                    IBinder mtdBinder = null;
                    if (AssetLocator.Type == GameType.DarkSoulsIII || AssetLocator.Type == GameType.Sekiro)
                    {
                        mtdBinder = BND4.Read(AssetLocator.GetAssetPath(@"mtd\allmaterialbnd.mtdbnd.dcx"));
                        IsMatbin = false;
                    }
                    else if (AssetLocator.Type is GameType.EldenRing or GameType.ArmoredCoreVI)
                    {
                        mtdBinder = BND4.Read(AssetLocator.GetAssetPath(@"material\allmaterial.matbinbnd.dcx"));
                        IsMatbin = true;
                    }

                    if (mtdBinder == null)
                    {
                        return;
                    }

                    if (IsMatbin)
                    {
                        _matbins = new Dictionary<string, MATBIN>();
                        foreach (BinderFile f in mtdBinder.Files)
                        {
                            var matname = Path.GetFileNameWithoutExtension(f.Name);
                            // Because *certain* mods contain duplicate entries for the same material
                            if (!_matbins.ContainsKey(matname))
                            {
                                _matbins.Add(matname, MATBIN.Read(f.Bytes));
                            }
                        }
                    }
                    else
                    {
                        _mtds = new Dictionary<string, MTD>();
                        foreach (BinderFile f in mtdBinder.Files)
                        {
                            var mtdname = Path.GetFileNameWithoutExtension(f.Name);
                            // Because *certain* mods contain duplicate entries for the same material
                            if (!_mtds.ContainsKey(mtdname))
                            {
                                _mtds.Add(mtdname, MTD.Read(f.Bytes));
                            }
                        }
                    }

                    mtdBinder.Dispose();
                }
                catch (Exception e) when (e is FileNotFoundException or DirectoryNotFoundException)
                {
                    TaskLogs.AddLog("Material files cannot not be found", Microsoft.Extensions.Logging.LogLevel.Warning, TaskLogs.LogPriority.Low);
                    _mtds = new Dictionary<string, MTD>();
                    _matbins = new Dictionary<string, MATBIN>();
                }
            }));
    }

    public static void LoadMtds(AssetLocator l)
    {
        AssetLocator = l;

        if (AssetLocator.Type == GameType.Undefined)
        {
            return;
        }

        ReloadMtds();
    }
}
