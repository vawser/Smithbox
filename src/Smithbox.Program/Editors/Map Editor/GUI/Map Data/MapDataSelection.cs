using SoulsFormats;
using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The category selection of the Map Data window.
/// The selected entries are the view's ViewportSelection, so they are shared with the other Map Editor windows.
/// </summary>
public class MapDataSelection
{
    public MapEditorView View;

    public string SelectedBaseCategory;
    public Type SelectedBaseCategoryType;

    public string SelectedSubCategory;
    public Type SelectedSubCategoryType;

    public MapContainer SelectedMap => View.Selection.SelectedMapContainer;
    public IMsb SourceMsb => SelectedMap?.SourceMsb;

    /// <summary>
    /// Routes and layers are not map entities, so they are listed but cannot be selected.
    /// </summary>
    public bool IsEntityCategory => SelectedBaseCategory is not ("Route" or "Layer");

    public MapDataSelection(MapEditorView view)
    {
        View = view;
    }

    public void ResetCategories()
    {
        SelectedBaseCategory = null;
        SelectedBaseCategoryType = null;
        SelectedSubCategory = null;
        SelectedSubCategoryType = null;
    }
}
