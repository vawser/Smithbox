using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class EntChangeTypeAction : ViewportAction
{
    private MapEditorView View;

    private readonly List<MsbEntity> Entities = new();
    private readonly List<MapObjectChange> MapObjectChanges = new();
    private readonly string MsbParamstr;
    private readonly Type MsbType;
    private readonly string[] NewTypes;
    private readonly string[] OldTypes;
    private readonly bool SetSelection;

    /// <summary>
    ///     Change selected map objects from one type to another. Only works for map objects of the same overarching type, such
    ///     as Parts or Regions.
    ///     Data for properties absent in targeted type will be lost, but will be restored for undo/redo.
    /// </summary>
    public EntChangeTypeAction(MapEditorView view, Type msbType, List<MsbEntity> selectedEnts, string[] oldTypes,
        string[] newTypes, string msbParamStr, bool setSelection)
    {
        View = view;
        MsbType = msbType;
        Entities.AddRange(selectedEnts);
        OldTypes = oldTypes;
        NewTypes = newTypes;
        SetSelection = setSelection;
        MsbParamstr = msbParamStr;

        var universe = View.Universe;

        // Go through applicable map entities and create WrappedObject with the new type for each.
        // Store entity, old obj, and new obj to be used when changing Entity's WrappedObject (including restoring the exact same objs in cases of undo/redo).
        for (var iType = 0; iType < OldTypes.Length; iType++)
        {
            // Get desired types for the current game's MSB
            Type sourceType = MsbType.GetNestedType(MsbParamstr).GetNestedType(OldTypes[iType]);
            Type targetType = MsbType.GetNestedType(MsbParamstr).GetNestedType(NewTypes[iType]);
            Type partType = MsbType.GetNestedType(MsbParamstr);

            foreach (MsbEntity ent in Entities)
            {
                Type currentType = ent.WrappedObject.GetType();
                if (currentType == sourceType)
                {
                    var sourceObj = ent.WrappedObject;

                    // Values are read from a deep copy, so the new object shares nothing (arrays, the class-typed
                    // sub-structs of later games) with the old one that undo puts back
                    object readFrom = sourceObj is IMsbPart part ? part.DeepCopy() : sourceObj;
                    var targetObj = targetType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());

                    // Go through properties of source type and set them to target type (if they exist under the same name)
                    // Public set properties
                    foreach (PropertyInfo property in sourceType.GetProperties().Where(p => p.CanWrite))
                    {
                        // Make sure target type has this property (DummyAssets are missing some properties)
                        PropertyInfo targetProp = targetObj.GetType().GetProperty(property.Name);
                        if (targetProp != null)
                        {
                            targetProp.SetValue(targetObj, CopyValue(property.GetValue(readFrom, null)), null);
                        }
                    }

                    // Private set properties
                    foreach (PropertyInfo property in sourceType.GetProperties().Where(p => !p.CanWrite))
                    {
                        // Make sure target type has this property (DummyAssets are missing some properties)
                        PropertyInfo targetProp = targetObj.GetType().GetProperty(property.Name);
                        if (targetProp != null)
                        {
                            PropertyInfo targetPropInner = targetProp.DeclaringType.GetProperty(property.Name);
                            targetPropInner.SetValue(targetObj, CopyValue(property.GetValue(readFrom, null)),
                                BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                        }
                    }

                    MapObjectChanges.Add(new MapObjectChange(sourceObj, targetObj, ent));
                }
            }
        }
    }

    /// <summary>
    /// Arrays and lists are copied, so the new object doesn't share them with the old one: an edit made after
    /// the change would otherwise also change the object that undo puts back
    /// </summary>
    private static object CopyValue(object value)
    {
        return value switch
        {
            Array array => array.Clone(),
            IList list when value.GetType().IsGenericType => Activator.CreateInstance(value.GetType(), list),
            _ => value
        };
    }

    /// <summary>
    /// The entity now wraps another object, so its model, its group arrays and its map's references are rebuilt
    /// </summary>
    private static void Refresh(IEnumerable<MsbEntity> entities)
    {
        foreach (var ent in entities)
        {
            ent.UpdateRenderModel();
        }

        foreach (var container in entities.Select(e => e.Container).Where(c => c != null).Distinct())
        {
            Entity.BuildReferenceMaps(container.Objects);
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (MapObjectChange mapChangeObj in MapObjectChanges)
        {
            // Assign new dummied/undummied wrappedObj to entity
            mapChangeObj.Entity.WrappedObject = mapChangeObj.NewObject;

            MapContainer.MarkEntityDirty(mapChangeObj.Entity);
        }

        Refresh(MapObjectChanges.Select(c => c.Entity));

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = View.Universe;

        foreach (MapObjectChange mapChangeObj in MapObjectChanges)
        {
            // Restore old, stored WrappedObject to entity.
            mapChangeObj.Entity.WrappedObject = mapChangeObj.OldObject;

            MapContainer.MarkEntityDirty(mapChangeObj.Entity);
        }

        Refresh(MapObjectChanges.Select(c => c.Entity));

        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
            foreach (MsbEntity ent in Entities)
            {
                universe.View.ViewportSelection.AddSelection(ent);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private record MapObjectChange(object OldObject, object NewObject, MsbEntity Entity);

}
