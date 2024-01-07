using ImGuiNET;
using System.Collections.Generic;

namespace StudioCore.MsbEditor;

public interface AssetBrowserEventHandler
{
    public void OnInstantiateChr(string chrid);
    public void OnInstantiateObj(string objid);
    public void OnInstantiateParts(string objid);
    public void OnInstantiateMapPiece(string mapid, string modelid);
}

public class AssetBrowser
{
    private readonly AssetBrowserEventHandler _handler;
    private readonly string _id;

    private readonly AssetLocator _locator;
    private List<string> _cacheFiltered = new();

    private List<string> _chrCache = new();
    private Dictionary<string, List<string>> _mapModelCache = new();
    private List<string> _objCache = new();
    private List<string> _partsCache = new();

    private string _searchStr = "";
    private string _searchStrCache = "";

    private string _selected;
    private string _selectedCache;

    public AssetBrowser(AssetBrowserEventHandler handler, string id, AssetLocator locator)
    {
        _id = id;
        _locator = locator;
        _handler = handler;
    }

    public void ClearCaches()
    {
        _chrCache = new List<string>();
        _objCache = new List<string>();
        _mapModelCache = new Dictionary<string, List<string>>();
        List<string> mapList = _locator.GetFullMapList();
        foreach (var m in mapList)
        {
            var adjm = _locator.GetAssetMapID(m);
            if (!_mapModelCache.ContainsKey(adjm))
            {
                _mapModelCache.Add(adjm, null);
            }
        }
    }

    public void OnGui()
    {
        if (ImGui.Begin($@"Asset Browser##{_id}"))
        {
            if (MapStudioNew.LowRequirementsMode)
            {
                ImGui.BeginDisabled();
            }

            ImGui.Columns(2);
            ImGui.BeginChild("AssetTypeList");
            if (ImGui.Selectable("Chr", _selected == "Chr"))
            {
                _chrCache = _locator.GetChrModels();
                _selected = "Chr";
            }

            var objLabel = "Obj";
            if (_locator.Type is GameType.EldenRing or GameType.ArmoredCoreVI)
            {
                objLabel = "Aeg";
            }

            if (ImGui.Selectable(objLabel, _selected == "Obj"))
            {
                _objCache = _locator.GetObjModels();
                _selected = "Obj";
            }

            if (ImGui.Selectable("Parts", _selected == "Parts"))
            {
                _partsCache = _locator.GetPartsModels();
                _selected = "Parts";
            }

            foreach (var m in _mapModelCache.Keys)
            {
                if (ImGui.Selectable(m, _selected == m))
                {
                    if (_mapModelCache[m] == null)
                    {
                        List<AssetDescription> modelList = _locator.GetMapModels(m);
                        var cache = new List<string>();
                        foreach (AssetDescription model in modelList)
                        {
                            cache.Add(model.AssetName);
                        }

                        _mapModelCache[m] = cache;
                    }

                    _selected = m;
                }
            }

            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("AssetListSearch");

            if (InputTracker.GetKeyDown(KeyBindings.Current.Map_PropSearch))
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.InputText($"Search <{KeyBindings.Current.Map_PropSearch.HintText}>", ref _searchStr, 255);

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.BeginChild("AssetList");

            if (_selected == "Chr")
            {
                if (_searchStr != _searchStrCache || _selected != _selectedCache)
                {
                    _cacheFiltered = _chrCache;
                    _searchStrCache = _searchStr;
                    _selectedCache = _selected;
                }

                foreach (var chr in _cacheFiltered)
                {
                    if (chr.Contains(_searchStr))
                    {
                        if (ImGui.Selectable(chr))
                        {
                        }

                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            _handler.OnInstantiateChr(chr);
                        }
                    }
                }
            }
            else if (_selected == "Obj")
            {
                if (_searchStr != _searchStrCache || _selected != _selectedCache)
                {
                    _cacheFiltered = _objCache;
                    _searchStrCache = _searchStr;
                    _selectedCache = _selected;
                }

                foreach (var obj in _cacheFiltered)
                {
                    if (obj.Contains(_searchStr))
                    {
                        if (ImGui.Selectable(obj))
                        {
                        }

                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            _handler.OnInstantiateObj(obj);
                        }
                    }
                }
            }
            else if (_selected == "Parts")
            {
                if (_searchStr != _searchStrCache || _selected != _selectedCache)
                {
                    _cacheFiltered = _partsCache;
                    _searchStrCache = _searchStr;
                    _selectedCache = _selected;
                }

                foreach (var part in _cacheFiltered)
                {
                    if (part.Contains(_searchStr))
                    {
                        if (ImGui.Selectable(part))
                        {
                        }

                        if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                        {
                            _handler.OnInstantiateParts(part);
                        }
                    }
                }
            }
            else if (_selected != null && _selected.StartsWith("m"))
            {
                if (_mapModelCache.ContainsKey(_selected))
                {
                    if (_searchStr != _searchStrCache || _selected != _selectedCache)
                    {
                        _cacheFiltered = _mapModelCache[_selected];
                        _searchStrCache = _searchStr;
                        _selectedCache = _selected;
                    }

                    foreach (var model in _cacheFiltered)
                    {
                        if (model.Contains(_searchStr))
                        {
                            if (ImGui.Selectable(model))
                            {
                            }

                            if (ImGui.IsItemClicked() && ImGui.IsMouseDoubleClicked(0))
                            {
                                _handler.OnInstantiateMapPiece(_selected, model);
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
            ImGui.EndChild();

            if (MapStudioNew.LowRequirementsMode)
            {
                ImGui.EndDisabled();
            }
        }
        
        ImGui.End();

    }
}
