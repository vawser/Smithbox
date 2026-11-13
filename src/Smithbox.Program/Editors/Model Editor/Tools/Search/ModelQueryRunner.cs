using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// A search in progress. The matches are passed over through the queue and picked up as the tool is drawn.
/// </summary>
public class ModelQueryRun
{
    public readonly CancellationTokenSource Cancellation = new();
    public readonly ConcurrentQueue<ModelQueryMatch> Found = new();

    public volatile int Progress;
    public volatile int Total;
    public volatile string Status = "";
    public volatile bool Done;
    public volatile bool Truncated;

    /// <summary>
    /// The most matches that are kept, or 0 for no limit
    /// </summary>
    public int MaxResults;

    public int Count;

    public void Cancel() => Cancellation.Cancel();
}

/// <summary>
/// Gathers the models to search on the main thread, and runs the search over them in the background. A global search
/// reads every model of the project, so it can take a while.
/// </summary>
public class ModelQueryRunner
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelQueryRunner(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    #region Sources
    /// <summary>
    /// The model lists a global search can cover, in the order the Source List shows them
    /// </summary>
    public static readonly ModelListType[] SearchableListTypes =
    [
        ModelListType.Character, ModelListType.Asset, ModelListType.Part, ModelListType.MapPiece
    ];

    /// <summary>
    /// The given models of one container. Models with edits in memory are searched as they are in the editor;
    /// the rest are read from the stored file.
    /// </summary>
    public ModelQuerySource GetLocalSource(ModelContainerWrapper container, IEnumerable<ModelWrapper> models)
    {
        var list = models.Select(m => new ModelQueryModel(m.Name, GetFlverInMemory(m))).ToList();
        var listType = GetListType(container);

        return new ModelQuerySource(container, listType, AliasHelper.GetAlias(Project, container.Name, listType), list, false);
    }

    /// <summary>
    /// Every container of the given lists (or all of them), each list in name order
    /// </summary>
    public List<ModelQuerySource> GetGlobalSources(ModelListType? onlyList)
    {
        var sources = new List<ModelQuerySource>();

        foreach (var listType in SearchableListTypes)
        {
            if (onlyList != null && onlyList != listType)
                continue;

            var bank = GetBankList(listType);

            if (bank == null)
                continue;

            foreach (var container in bank.Values.OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase))
            {
                // Models with edits in memory are searched as they are in the editor, the rest read again
                var inMemory = container.Models
                    .Select(m => new ModelQueryModel(m.Name, GetFlverInMemory(m)))
                    .Where(m => m.Flver != null)
                    .ToList();

                sources.Add(new ModelQuerySource(container, listType, AliasHelper.GetAlias(Project, container.Name, listType),
                    inMemory, true));
            }
        }

        return sources;
    }

    /// <summary>
    /// The model as the editor holds it, when it has edits in memory (loaded, parked or unsaved); else null, for it
    /// to be read from its file
    /// </summary>
    private static FLVER2 GetFlverInMemory(ModelWrapper wrapper)
    {
        if (wrapper.FLVER != null && (wrapper.IsDirty || wrapper.Container != null || wrapper.Parked != null))
            return wrapper.GetFlverForReading();

        return null;
    }

    private Dictionary<FileDictionaryEntry, ModelContainerWrapper> GetBankList(ModelListType listType)
    {
        var bank = Project.Handler.ModelData.PrimaryBank;

        return listType switch
        {
            ModelListType.Character => bank.Characters,
            ModelListType.Asset => bank.Assets,
            ModelListType.Part => bank.Parts,
            ModelListType.MapPiece => bank.MapPieces,
            _ => null
        };
    }

    public ModelListType GetListType(ModelContainerWrapper container)
    {
        foreach (var listType in SearchableListTypes)
        {
            if (GetBankList(listType)?.ContainsValue(container) == true)
                return listType;
        }

        return ModelListType.Character;
    }
    #endregion

    #region Run
    /// <summary>
    /// Starts the search on a background task. The progress counts the models when countModels is set, for a search
    /// of one container, or else the containers.
    /// </summary>
    public ModelQueryRun Start(IModelQueryFilter query, List<ModelQuerySource> sources, bool countModels, int maxResults)
    {
        var run = new ModelQueryRun
        {
            MaxResults = maxResults,
            Total = countModels ? sources.Sum(e => e.Models.Count) : sources.Count
        };

        var token = run.Cancellation.Token;

        Task.Run(() =>
        {
            try
            {
                Run(run, query, sources, countModels, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MODEL_SearchTool_Failed"), e);
            }
            finally
            {
                run.Done = true;
            }
        }, token);

        return run;
    }

    private void Run(ModelQueryRun run, IModelQueryFilter query, List<ModelQuerySource> sources, bool countModels,
        CancellationToken token)
    {
        foreach (var source in sources)
        {
            token.ThrowIfCancellationRequested();

            run.Status = source.Container.Name;

            foreach (var (name, flver) in GetModels(query, source, token))
            {
                token.ThrowIfCancellationRequested();

                if (countModels)
                {
                    run.Status = $"{source.Container.Name} {name}";
                }

                if (flver != null)
                {
                    try
                    {
                        if (!QueryModel(run, query, source, name, flver, token))
                            return;
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception e)
                    {
                        // A loaded model can be edited while it is searched, which can upset the lists being read
                        Smithbox.LogError(this, LOC.Get("MODEL_SearchTool_Model_Failed", source.Container.Name, name), e);
                    }
                }

                if (countModels)
                {
                    run.Progress++;
                }
            }

            if (!countModels)
            {
                run.Progress++;
            }
        }
    }

    /// <summary>
    /// The models of a source with their FLVERs: those held in memory as they are, the rest read from the stored file.
    /// Models the model input rules out aren't read at all. A model that can't be read is given with a null FLVER, so
    /// it is still counted.
    /// </summary>
    private IEnumerable<(string Name, FLVER2 Flver)> GetModels(IModelQueryFilter query, ModelQuerySource source, CancellationToken token)
    {
        var given = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // What still has to be read from the file, by model name
        var toRead = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var model in source.Models)
        {
            given.Add(model.Name);

            if (model.Flver == null)
            {
                toRead.Add(model.Name);
            }
        }

        foreach (var model in source.Models)
        {
            if (model.Flver != null && query.MatchModel(model.Name, source))
                yield return (model.Name, model.Flver);
        }

        if (toRead.Count == 0 && !source.ReadOthers)
            yield break;

        foreach (var model in ReadModels(query, source, given, toRead, token))
            yield return model;
    }

    private IEnumerable<(string Name, FLVER2 Flver)> ReadModels(IModelQueryFilter query, ModelQuerySource source,
        HashSet<string> given, HashSet<string> toRead, CancellationToken token)
    {
        var container = source.Container;
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, container.Path);

        // A loose model file holds the one model, named after it
        if (binderType is ResourceContainerType.None)
        {
            var name = container.Name;

            if ((toRead.Contains(name) || source.ReadOthers && !given.Contains(name)) &&
                query.MatchModel(name, source))
            {
                yield return (name, ReadLooseFlver(container));
            }

            yield break;
        }

        IBinder binder = null;

        try
        {
            binder = container.ReadContainer();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_Binder_File", container.Path), e);
        }

        if (binder == null)
            yield break;

        using (binder)
        {
            foreach (var file in binder.Files)
            {
                token.ThrowIfCancellationRequested();

                var filepath = file.Name.ToLower();

                if (!filepath.Contains(".flver") && !filepath.Contains(".flv"))
                    continue;

                var name = FilePathUtils.GetPureFilename(file.Name);

                if (!toRead.Contains(name) && (!source.ReadOthers || given.Contains(name)))
                    continue;

                // The name is known before the model is read, so models the input rules out cost nothing more
                if (!query.MatchModel(name, source))
                    continue;

                FLVER2 flver = null;

                try
                {
                    flver = Flver0Converter.ReadEditorFlver(Project, file.Bytes);
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_FLVER", file.Name), e);
                }

                yield return (name, flver);
            }
        }
    }

    private FLVER2 ReadLooseFlver(ModelContainerWrapper container)
    {
        try
        {
            var data = container.TargetFS.ReadFile(container.Path);
            return data == null ? null : Flver0Converter.ReadEditorFlver(Project, data.Value);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Data_Failed_To_Read_FLVER", container.Path), e);
            return null;
        }
    }

    /// <summary>
    /// Returns false once there are as many matches as are kept
    /// </summary>
    private bool QueryModel(ModelQueryRun run, IModelQueryFilter query, ModelQuerySource source, string modelName, FLVER2 flver,
        CancellationToken token)
    {
        // Nothing asked of the objects: the model is the match
        if (!query.MatchesObjects)
        {
            return AddMatch(run, new ModelQueryMatch(source.Container, source.ListType, modelName,
                ModelQueryObjectKind.Header, -1, flver.Header, modelName));
        }

        // The lists are copied, as a loaded model can be edited while it is searched
        var objects = ModelQueryMatcher.GetObjects(flver).ToList();

        foreach (var (kind, index, obj) in objects)
        {
            token.ThrowIfCancellationRequested();

            var objectName = ModelQueryMatcher.GetObjectName(kind, obj, index);

            if (!query.MatchObject(kind, obj, objectName))
                continue;

            if (!query.MatchFields(obj, out var fieldMatch))
                continue;

            var objectLabel = string.IsNullOrEmpty(objectName)
                ? ModelQueryMatcher.GetKindName(kind)
                : $"{ModelQueryMatcher.GetKindName(kind)} {objectName}";

            var label = fieldMatch != null ? $"{objectLabel}  {fieldMatch}" : objectLabel;

            if (!AddMatch(run, new ModelQueryMatch(source.Container, source.ListType, modelName, kind, index, obj, label)))
                return false;
        }

        return true;
    }

    private static bool AddMatch(ModelQueryRun run, ModelQueryMatch match)
    {
        if (run.MaxResults > 0 && run.Count >= run.MaxResults)
        {
            run.Truncated = true;
            return false;
        }

        run.Count++;
        run.Found.Enqueue(match);

        return true;
    }
    #endregion

    #region Navigation
    /// <summary>
    /// Loads the model of a match if it isn't loaded, then selects the object found and brings it into view in
    /// Contents. A model loaded (or kept with its history) in another view is selected there instead.
    /// </summary>
    public void Jump(ModelQueryMatch match)
    {
        var container = match.Container;

        var wrapper = FindModel(container, match.ModelName);

        if (wrapper == null)
        {
            // The list of models is only read once the container is picked
            container.PopulateModelList();
            wrapper = FindModel(container, match.ModelName);
        }

        if (wrapper == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_SearchTool_Stale_Result"));
            return;
        }

        var view = wrapper.Container?.View ?? wrapper.Parked?.View ?? View;

        if (view.Selection.SelectedModelWrapper != wrapper)
        {
            view.Selection.SelectedModelContainerWrapper = container;

            if (!view.LoadModel(wrapper))
                return;
        }
        else
        {
            view.Selection.SelectedModelContainerWrapper = container;
        }

        var target = Resolve(wrapper.Container, match);

        if (target == null)
        {
            // The object may have been removed since, or moved by an edit
            Smithbox.LogError(this, LOC.Get("MODEL_SearchTool_Stale_Result"));
            return;
        }

        view.ViewportSelection.ClearSelection();
        view.ViewportSelection.AddSelection(target);
        view.ViewportSelection.GotoTreeTarget = target;
    }

    private static ModelWrapper FindModel(ModelContainerWrapper container, string name)
    {
        return container.Models.FirstOrDefault(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Finds what a match points to among the objects of the loaded model: the object searched, if the model was
    /// searched as it is loaded, else the object at the same position. Null if there is none of the kind there.
    /// </summary>
    public static Entity Resolve(ModelContainer container, ModelQueryMatch match)
    {
        if (container == null)
            return null;

        if (match.Kind is ModelQueryObjectKind.Header)
            return container.RootObject;

        var list = GetEntityList(container, match.Kind);

        if (list == null)
            return null;

        var entity = list.FirstOrDefault(e => ReferenceEquals(e.WrappedObject, match.Object));

        if (entity == null && match.Index >= 0 && match.Index < list.Count)
        {
            entity = list[match.Index];
        }

        return entity;
    }

    private static List<Entity> GetEntityList(ModelContainer container, ModelQueryObjectKind kind) => kind switch
    {
        ModelQueryObjectKind.Dummy => container.Dummies,
        ModelQueryObjectKind.Material => container.Materials,
        ModelQueryObjectKind.Node => container.Nodes,
        ModelQueryObjectKind.Mesh => container.Meshes,
        _ => null
    };

    /// <summary>
    /// Whether the match is what the view has selected
    /// </summary>
    public static bool IsSelected(ModelEditorView view, ModelQueryMatch match)
    {
        var wrapper = view.Selection.SelectedModelWrapper;

        if (wrapper?.Container == null || wrapper.Parent != match.Container ||
            !string.Equals(wrapper.Name, match.ModelName, StringComparison.OrdinalIgnoreCase))
            return false;

        var selected = view.ViewportSelection.GetSingleSelection() as Entity;

        if (selected == null)
            return false;

        if (match.Kind is ModelQueryObjectKind.Header)
            return selected == wrapper.Container.RootObject;

        if (ReferenceEquals(selected.WrappedObject, match.Object))
            return true;

        var list = GetEntityList(wrapper.Container, match.Kind);

        return list != null && match.Index >= 0 && match.Index < list.Count && list[match.Index] == selected;
    }
    #endregion
}
