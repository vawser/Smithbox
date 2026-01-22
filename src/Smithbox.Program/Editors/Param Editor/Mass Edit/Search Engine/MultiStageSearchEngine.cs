using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

/*
 *  Handles conversion to a secondary searchengine which handles && conditions and conversion back to the anticipated type
 */
public class MultiStageSearchEngine<A, B, C, D> : SearchEngine<A, B>
{
    internal Func<A, B, C> contextGetterForMultiStage;
    internal Func<D, B, B> resultRetrieverForMultiStage;
    internal SearchEngine<C, D> searchEngineForMultiStage;
    internal Func<B, D> sourceListGetterForMultiStage;

    public override List<B> Search(A context, List<B> sourceSet, string command, bool lenient,
        bool failureAllOrNone)
    {
        var conditions = command.Split("&&", 2, StringSplitOptions.TrimEntries);
        List<B> stage1list = base.Search(context, sourceSet, conditions[0], lenient, failureAllOrNone);
        if (conditions.Length == 1)
        {
            return stage1list;
        }

        B exampleItem = stage1list.FirstOrDefault();
        List<D> stage2list = searchEngineForMultiStage.Search(contextGetterForMultiStage(context, exampleItem),
            stage1list.Select(x => sourceListGetterForMultiStage(x)).ToList(), conditions[1], lenient,
            failureAllOrNone);
        return stage2list.Select(x => resultRetrieverForMultiStage(x, exampleItem)).ToList();
    }
}