using Andre.Core;
using Andre.IO.VFS;
using DotNext.Collections.Generic;
using SoulsFormats;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.IO
{
    public static partial class Locator
    {
        /// <summary>
        /// Given a map's numerical ID, finds its path.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the path ought to be even if the file doesn't exist</param>
        /// <returns></returns>
        public static AssetLocation? FindMsbForId(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            if (mapid.Length != 12)
                return null;
            string? path = game switch
            {
                Game.DS2S or Game.DS2 => $"map/{mapid}/{mapid}.msb",
                Game.BB when mapid.StartsWith("m29") => $@"map/MapStudio/{mapid[..9]}_00/{mapid}.msb",
                Game.DS1 or Game.DS1R or Game.DES or Game.BB or Game.DS3 or Game.ER or Game.AC6
                    => $@"map/MapStudio/{mapid}.msb",
                _ => null
            };
            if (path == null) return null;

            bool exists = fs.FileExists(path);
            bool dcxExists = fs.FileExists(path + ".dcx");
            bool preferNoDcx = game is Game.DS1 or Game.DS2 or Game.DS2S or Game.DS1R or Game.DES;
            if ((writemode && preferNoDcx) || (!dcxExists && exists))
                return new(path, mapid);
            if (writemode || dcxExists)
                return new(path + ".dcx", mapid);
            return null;
        }

        /// <summary>
        /// Gets the paths of all BTLs for the given map ID.
        /// Returns an empty List if no BTLs are found.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the paths ought to be even if the files don't exist</param>
        /// <returns></returns>
        public static List<AssetLocation> GetMapBtls(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            List<AssetLocation> ans = [];
            if (mapid.Length != 12)
                return ans;
            if (game is Game.DS2S or Game.DS2)
            {
                //DS2 BTLs are located inside .gibdt files
                string name = $"g{mapid[1..]}";
                string gibhd = $@"model/map/g{mapid[1..]}.gibhd";
                if (fs.FileExists(gibhd) || writemode)
                {
                    var bdt = new AssetLocation(gibhd.Replace(".gibhd", ".gibdt"), name);
                    var bhd = new BxfLocation(gibhd, Path.GetFileName(gibhd), bdt,
                        $"model/map/{mapid}/");
                    ans.Add(new(bhd, $"model/map/{mapid}/light.btl.dcx", name));
                }

                gibhd = $"model_lq/map/g{mapid[1..]}.gibhd";
                if (fs.FileExists(gibhd) || writemode)
                {
                    var bdt = new AssetLocation(gibhd.Replace(".gibhd", ".gibdt"), name);
                    var bhd = new BxfLocation(gibhd, Path.GetFileName(gibhd), bdt,
                        $"model_lq/map/{mapid}/");
                    ans.Add(new(bhd, $"model_lq/map/{mapid}/light.btl.dcx", name));
                }
            }
            else if (game is Game.BB or Game.DS3 or Game.SDT or Game.ER or Game.AC6)
            {
                string folder;
                if (game is Game.ER or Game.AC6)
                    folder = $"map/{mapid[..3]}/{mapid}";
                else
                    folder = $"map/{mapid}";

                var names = fs.GetFileNamesWithExtensions(folder, ".btl", ".btl.dcx")
                    .Select(Path.GetFileName)
                    .Select(s => s?.Replace(".dcx", "").Replace(".btl", "") ?? "")
                    .Distinct();
                foreach (var name in names)
                {
                    string regularPath = $"{folder}/{name}.btl";
                    bool exists = fs.FileExists(regularPath);
                    string dcxPath = $"{regularPath}.dcx";
                    bool dcxExists = fs.FileExists(dcxPath);

                    string? path = null;
                    if (dcxExists || writemode)
                        path = dcxPath;
                    else if (exists)
                        path = regularPath;

                    if (path != null)
                    {
                        ans.Add(new(path, name));
                    }
                }
            }

            return ans;
        }

        /// <summary>
        /// Gets the paths of all BTABs for the given map ID.
        /// Returns an empty List if no BTABs are found.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the paths ought to be even if the files don't exist</param>
        /// <returns></returns>
        public static List<AssetLocation> GetMapBtabs(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            List<AssetLocation> ans = [];
            if (mapid.Length != 12)
                return ans;
            if (game is Game.DS2S or Game.DS2)
            {
                //DS2 BTABs are located inside .gibdt files
                string name = $"g{mapid[1..]}";
                string gibhd = $@"model/map/g{mapid[1..]}.gibhd";
                if (fs.FileExists(gibhd) || writemode)
                {
                    var bdt = new AssetLocation(gibhd.Replace(".gibhd", ".gibdt"), name);
                    var bhd = new BxfLocation(gibhd, Path.GetFileName(gibhd), bdt,
                        $"model/map/{mapid}/");
                    ans.Add(new(bhd, $"model/map/{mapid}/atlasinfo.btab.dcx", name));
                }

                gibhd = $"model_lq/map/g{mapid[1..]}.gibhd";
                if (fs.FileExists(gibhd) || writemode)
                {
                    var bdt = new AssetLocation(gibhd.Replace(".gibhd", ".gibdt"), name);
                    var bhd = new BxfLocation(gibhd, Path.GetFileName(gibhd), bdt,
                        $"model_lq/map/{mapid}/");
                    ans.Add(new(bhd, $"model_lq/map/{mapid}/atlasinfo.btab.dcx", name));
                }
            }
            else if (game is Game.BB or Game.DS3 or Game.SDT)
            {
                string folder = $"map/{mapid}";

                var locs = fs.GetFileNamesWithExtensions(folder, ".btab.dcx")
                    .Select(p => new AssetLocation(p,
                        Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(p))));
                ans.AddRange(locs);
            }

            return ans;
        }

        /// <summary>
        /// Gets the location of the NVA corresponding to a map id.
        /// Returns null if no NVA is found.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <param name="writemode">If true, returns what the path ought to be even if the file doesn't exist</param>
        /// <returns></returns>
        public static AssetLocation? GetMapNva(string mapid, Game game, VirtualFileSystem fs, bool writemode = false)
        {
            if (mapid.Length != 12) return null;
            if (game is not (Game.BB or Game.DS3 or Game.SDT or Game.ER or Game.AC6))
                return null;

            string path = game switch
            {
                //BB chalice maps
                Game.BB when mapid.StartsWith("m29") => $"map/{mapid[..9]}_00/{mapid}.nva",
                Game.ER => $"map/{mapid[..3]}/{mapid}/{mapid}.nva",
                _ => $"map/{mapid}/{mapid}.nva"
            };

            string dcxPath = $"{path}.dcx";
            string ansPath;
            if (fs.FileExists(dcxPath) || writemode)
                ansPath = dcxPath;
            else if (fs.FileExists(path))
                ansPath = path;
            else
                return null;

            return new(ansPath, mapid);
        }

        public static (AssetLocation? hi, AssetLocation? lo) GetMapCollision(string mapid, string model, VirtualFileSystem fs, Game game)
        {
            if (mapid.Length != 12)
                return (null, null);
            string tmp;
            string mapidNoM = mapid[1..];
            string modelNoH = model[1..];
            AssetLocation? hi = null;
            AssetLocation? lo = null;
            if (game is Game.DS1 or Game.DES or Game.DS1R)
            {
                string noBhdPath = $"map/{mapid}/HIORLO{modelNoH}.hkx";
                tmp = noBhdPath.Replace("HIORLO", "h");
                if (fs.FileExists(tmp))
                    hi = new(tmp, Path.GetFileNameWithoutExtension(tmp));
                tmp = noBhdPath.Replace("HIORLO", "l");
                if (fs.FileExists(tmp))
                    lo = new(tmp, Path.GetFileNameWithoutExtension(tmp));
                if (hi is not null && lo is not null)
                    return (hi, lo);

            }

            string? hkxbhd = game switch
            {
                Game.DS1 or Game.DS1R or Game.DES => $"map/{mapid}/HIORLO{mapidNoM}.hkxbhd",
                Game.DS2S => $"model/map/HIORLO{mapidNoM}.hkxbhd",
                Game.DS3 or Game.BB => $"map/{mapid}/HIORLO{mapidNoM}.hkxbhd",
                Game.ER => $"map/{mapid[..3]}/{mapid}/HIORLO{mapidNoM}.hkxbhd",
                _ => null
            };
            if (hkxbhd is null)
                return (hi, lo);
            if (hi is null)
            {
                tmp = hkxbhd.Replace("HIORLO", "h");
                if (fs.FileExists(tmp))
                {
                    var name = Path.GetFileNameWithoutExtension(tmp);
                    var bdt = new AssetLocation(tmp.Replace("hkxbhd", "hkxbdt"), name);
                    var bhd = new BxfLocation(tmp, name, bdt,
                        $"map/{mapid}/hit/hi");
                    hi = new(bhd,
                        $"{bhd.ArchiveVirtualPath}/h{modelNoH}.hkx", $"h{modelNoH}");
                }
            }
            if (lo is null)
            {
                tmp = hkxbhd.Replace("HIORLO", "l");
                if (fs.FileExists(tmp))
                {
                    var name = Path.GetFileNameWithoutExtension(tmp);
                    var bdt = new AssetLocation(tmp.Replace("hkxbhd", "hkxbdt"), name);
                    var bhd = new BxfLocation(tmp, name, bdt,
                        $"map/{mapid}/hit/lo");
                    hi = new(bhd,
                        $"{bhd.ArchiveVirtualPath}/l{modelNoH}.hkx", $"l{modelNoH}");
                }
            }

            return (hi, lo);
        }

        private static readonly LocatorCacheList<string, AssetLocation> mapTextureBinders = new();

        /// <summary>
        /// For DS1, mapid is the 3-character map id, e.g. "m10".
        /// For other games, mapid is the 12-character map id, e.g. "m10_00_00_00"
        /// </summary>
        /// <param name="mapid"></param>
        /// <returns></returns>
        public static AssetLocation? GetMapTextureBinders(string mapid, VirtualFileSystem fs, Game game)
        {
            if (game is Game.DS1)
            {
                if (mapid.Length > 3) mapid = mapid[..3];
                else if (mapid.Length < 3) return null;
            }
            else
            {
                if (mapid.Length != 12) return null;
            }
            var cache = mapTextureBinders[fs];
            if (cache.TryGetValue(mapid, out var ret))
                return ret;
            if (game is Game.DS1)
            {
                List<BxfLocation> bxfs = [];
                foreach (var tpfbhd in fs.GetFileNamesWithExtensions($"map/{mapid}", "tpfbhd"))
                {
                    var name = Path.GetFileNameWithoutExtension(tpfbhd);
                    AssetLocation bdt = new(tpfbhd.Replace(".tpfbhd", ".tpfbdt"), name);
                    bxfs.Add(new(tpfbhd, name, bdt, $"map/{mapid}"));
                }

                ret = new CombinedBxfLocation(mapid, bxfs.ToArray());
                cache.Add(mapid, ret);
                return ret;
            }

            return null;
        }

        private static LocatorCacheList<string, BndLocation> objBndCache = new();

        public static BndLocation? GetObjectBndLocation(string obj, VirtualFileSystem fs, Game game)
        {
            var cache = objBndCache[fs];
            if (cache.TryGetValue(obj, out var bnd))
                return bnd;
            if (game is Game.DS1 or Game.DS1R)
            {
                string bndPath = $"obj/{obj}.objbnd";
                if (!fs.FileExists(bndPath)) bndPath = bndPath + ".dcx";
                if (!fs.FileExists(bndPath)) return null;
                bnd = new(bndPath, obj, $"obj/{obj}");
                cache.Add(obj, bnd);
                return bnd;
            }

            return null;
        }
        public static (AssetLocation? model, MaybeLocation<AssetLocation>? collisionHi, MaybeLocation<AssetLocation>? collisionLo)
            GetObjectLocation(string obj, VirtualFileSystem fs, Game game)
        {
            if (game is Game.DS1 or Game.DS1R)
            {
                var bnd = GetObjectBndLocation(obj, fs, game);
                var model = new AssetLocation(bnd, $"obj/{obj}/{obj}.flver", obj);
                var collision = new AssetLocation(bnd, $"obj/{obj}/{obj}.hkx", obj);
                return (model, new(collision), null);
            }

            if (game is Game.ER)
            {
                string aegFolder = obj[..6];
                string geombndPath = $"asset/aeg/{aegFolder}/{obj}.geombnd.dcx";
                if (!fs.FileExists(geombndPath))
                    return (null, null, null);
                var geombnd = new BndLocation(geombndPath, obj,
                    $"obj/{obj}");
                var model = new AssetLocation(geombnd, $"obj/{obj}/{obj}.flver", obj);
                var hibndPath = $"asset/aeg/{aegFolder}/{obj}_h.geomhkxbnd.dcx";
                MaybeLocation<AssetLocation>? collisionHi = null;
                if (fs.FileExists(hibndPath))
                {
                    var hibnd = new BndLocation(hibndPath, obj, $"obj/{obj}");
                    collisionHi = new(new AssetLocation(hibnd, $"obj/{obj}/model/{obj}_h.hkx", obj));
                }
                var lobndPath = $"asset/aeg/{aegFolder}/{obj}_l.geomhkxbnd";
                MaybeLocation<AssetLocation>? collisionLo = null;
                if (fs.FileExists(hibndPath))
                {
                    var lobnd = new BndLocation(hibndPath, obj, $"obj/{obj}");
                    collisionLo = new(new AssetLocation(lobnd, $"obj/{obj}/{obj}_l.hkx", obj));
                }

                return (model, collisionHi, collisionLo);
            }

            return (null, null, null);
        }

        public static List<AssetLocation> GetObjectTextureLocations(AssetLocation modelLocation, FLVER2 model,
            VirtualFileSystem fs, Game game)
        {
            List<AssetLocation> ans = [];
            string obj = modelLocation.AssetName!;
            if (game is Game.DS1 or Game.DS1R)
            {
                var objbnd = modelLocation.ContainingArchive;
                if (objbnd is null) throw new FileNotFoundException("Expected model to be in objbnd");
                var objtpf = new TpfLocation(objbnd, $"obj/{obj}/{obj}.tpf", obj, "tex");
                foreach (var m in model.Materials)
                {
                    if (m == null) continue;
                    foreach (var t in m.Textures)
                    {
                        if (t == null || t.Path == "") continue;
                        var p = t.Path.ToLower();
                        if (p.Contains($@"obj\{obj}\tex"))
                        {
                            var fname = Path.GetFileNameWithoutExtension(p);
                            ans.Add(new(objtpf, $"obj/{obj}/tex/{fname}", fname));
                        }
                        else if (Regex.IsMatch(p, @".*obj\\o\d+\\tex"))
                        {
                            var fname = Path.GetFileNameWithoutExtension(p);
                            var objn = Regex.Match(p, @".*obj\\(o\d+)\\tex").Groups[1].Value;
                            var bnd = GetObjectBndLocation(objn, fs, game);
                            var tpf = new TpfLocation(bnd, $"obj/{objn}/{objn}.tpf", objn, "tex");
                            ans.Add(new(tpf, $"obj/{objn}/tex/{fname}", fname));
                        }
                        else if (p.Contains("model\\map"))
                        {
                            var mapid = Regex.Match(p, @".*model\\map\\(m\d\d).*").Groups[1].Value;
                            var fname = Path.GetFileNameWithoutExtension(p);
                            TpfLocation tpf;
                            if (fs.DirectoryExists("map/tx"))
                            {
                                tpf = new TpfLocation($"map/tx/{fname}.tpf", fname,
                                    $"map/{mapid}/tex");
                            }
                            else
                            {
                                tpf = new TpfLocation(GetMapTextureBinders(mapid, fs, game),
                                    $"map/{mapid}/{fname}.tpf", fname, "tex");
                            }
                            ans.Add(new(tpf, $"map/{mapid}/tex/{fname}", fname));
                        }
                        else if (p.Contains("sfx\\tex"))
                        {
                            var fname = Path.GetFileNameWithoutExtension(p);
                            var sfxbnd = new BndLocation("sfx/frpg_sfxbnd_commoneffects.ffxbnd",
                                "FRPG_SfxBnd_CommonEffects", "sfx");
                            var tpf = new TpfLocation(sfxbnd, $"sfx/{fname}.tpf", fname, "tex");
                            ans.Add(new(tpf, $"sfx/tex/{fname}", fname));
                        }
                        else
                        {
                            Console.WriteLine($"Could not locate texture {p}");
                        }
                    }
                }
            }
            return ans;
        }

        /// <summary>
        /// Gets the IDs for all maps that exist in a given filesystem.
        /// Note that in some cases, these IDs may need to be adjusted to find their assets.
        /// </summary>
        /// <param name="game"></param>
        /// <param name="fs"></param>
        /// <returns></returns>
        public static List<string> GetAllMapIDs(Game game, VirtualFileSystem fs)
        {
            HashSet<string> mapSet = [];
            if (game is Game.DS2S or Game.DS2)
            {
                mapSet.AddAll(fs.GetFileSystemEntriesMatching("map", "m.*")
                    .Select(Path.GetFileNameWithoutExtension)
                    .SkipNulls());
            }
            else
            {
                mapSet.AddAll(fs.GetFileNamesWithExtensions("map/MapStudio", ".msb")
                    .Select(Path.GetFileNameWithoutExtension)
                    .SkipNulls());
                mapSet.AddAll(fs.GetFileNamesWithExtensions("map/MapStudio", ".msb.dbx")
                    .Select(Path.GetFileNameWithoutExtension).Select(Path.GetFileNameWithoutExtension)
                    .SkipNulls());
            }
            var mapRegex = MapIdRegex();
            return mapSet.Where((s) => mapRegex.IsMatch(s)).Order().ToList();
        }

        [GeneratedRegex(@"^m\d{2}_\d{2}_\d{2}_\d{2}$")]
        private static partial Regex MapIdRegex();

        /// <summary>
        /// Given a map ID, this function gets the map ID its assets are stored under.
        /// This is necessary because in some cases, maps store their assets under a different map ID.
        /// </summary>
        /// <param name="mapid"></param>
        /// <param name="game"></param>
        /// <returns></returns>
        public static string GetAssetMapId(string mapid, Game game)
        {
            return game switch
            {
                Game.ER or Game.AC6 or Game.DES => mapid,
                //DSR m99 maps contain their own assets
                Game.DS1R when mapid.StartsWith("m99") => mapid,
                //BB chalice dungeons
                Game.BB when mapid.StartsWith("m29") => "m29_00_00_00",
                _ => mapid[..6] + "_00_00"
            };
        }

    }

    internal class LocatorCacheList<K, V> where K : notnull
    {
        private Dictionary<VirtualFileSystem, LocatorCache<K, V>> caches = [];

        public void Add(VirtualFileSystem k, LocatorCache<K, V> v) => caches.Add(k, v);
        public void Remove(VirtualFileSystem k) => caches.Remove(k);

        public LocatorCache<K, V> this[VirtualFileSystem index]
        {
            get
            {
                if (caches.TryGetValue(index, out var c))
                    return c;
                c = new(this, index);
                return c;
            }
        }
    }

    internal class LocatorCache<K, V> : Dictionary<K, V>, IDisposable where K : notnull
    {
        private readonly LocatorCacheList<K, V> cacheDict;
        private readonly VirtualFileSystem fs;
        public LocatorCache(LocatorCacheList<K, V> cacheDict, VirtualFileSystem fs)
        {
            cacheDict.Add(fs, this);
            this.cacheDict = cacheDict;
            this.fs = fs;
            fs.AddDisposable(this);
        }
        public void Dispose()
        {
            cacheDict.Remove(fs);
            foreach (var kv in this)
            {
                if (kv.Value is IDisposable d) d.Dispose();
            }
        }
    }

}