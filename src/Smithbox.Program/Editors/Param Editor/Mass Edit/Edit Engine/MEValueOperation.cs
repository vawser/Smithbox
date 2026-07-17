using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class MEValueOperation : MEOperation<object, object>
{
    public ParamEditorView CurrentView;

    public MEValueOperation(ParamEditorView curView)
    {
        CurrentView = curView;
        operations.Clear();
        Setup();
    }

    internal override void Setup()
    {
        operations.Add("=", (new[] {
            LOC.Get("PARAM_CellOp_Equals_Hint_1")},
            LOC.Get("PARAM_CellOp_Equals_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => args[0])));

        operations.Add("+", (new[] {
            LOC.Get("PARAM_CellOp_Addition_Hint_1")},
            LOC.Get("PARAM_CellOp_Addition_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v =>
            {
                double val;
                if (double.TryParse(args[0], out val))
                {
                    return v + val;
                }

                return v + args[0];
            })));

        operations.Add("-", (new[] {
            LOC.Get("PARAM_CellOp_Subtract_Hint_1")},
            LOC.Get("PARAM_CellOp_Subtract_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v - double.Parse(args[0]))));

        operations.Add("*", (new[] {
            LOC.Get("PARAM_CellOp_Multiply_Hint_1")},
            LOC.Get("PARAM_CellOp_Multiply_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v * double.Parse(args[0]))));

        operations.Add("/", (new[] {
            LOC.Get("PARAM_CellOp_Divide_Hint_1")},
            LOC.Get("PARAM_CellOp_Divide_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v / double.Parse(args[0]))));

        operations.Add("%", (new[] {
            LOC.Get("PARAM_CellOp_Modulo_Hint_1")},
            LOC.Get("PARAM_CellOp_Modulo_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v % double.Parse(args[0]))));

        operations.Add("scale", (new[] {
            LOC.Get("PARAM_CellOp_Scale_Hint_1"),
            LOC.Get("PARAM_CellOp_Scale_Hint_2")},
            LOC.Get("PARAM_CellOp_Scale_TT"),
            (ctx, args) =>
            {
                var opp1 = double.Parse(args[0]);
                var opp2 = double.Parse(args[1]);
                return MassParamEdit.WithDynamicOf(ctx, v =>
                {
                    return ((v - opp2) * opp1) + opp2;
                });
            }
        ));

        operations.Add("replace", (new[] { 
            LOC.Get("PARAM_CellOp_Replace_Hint_1"),
            LOC.Get("PARAM_CellOp_Replace_Hint_2")},
            LOC.Get("PARAM_CellOp_Replace_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v.Replace(args[0], args[1]))));

        operations.Add("replacex", (new[] {
            LOC.Get("PARAM_CellOp_ReplaceEx_Hint_1"),
            LOC.Get("PARAM_CellOp_ReplaceEx_Hint_2")},
            LOC.Get("PARAM_CellOp_ReplaceEx_TT"),
            (ctx, args) =>
            {
                Regex rx = new(args[0]);
                return MassParamEdit.WithDynamicOf(ctx, v => rx.Replace(v, args[1]));
            }
        ));

        operations.Add("max", (new[] { 
            LOC.Get("PARAM_CellOp_Max_Hint_1") }, 
            LOC.Get("PARAM_CellOp_Max_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Max(v, double.Parse(args[0])))));

        operations.Add("min", (new[] { 
            LOC.Get("PARAM_CellOp_Min_Hint_1")}, 
            LOC.Get("PARAM_CellOp_Min_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Min(v, double.Parse(args[0])))));

        operations.Add("round", (new[] { 
            LOC.Get("PARAM_CellOp_Round_Hint_1")}, 
            LOC.Get("PARAM_CellOp_Round_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Round(v, int.Parse(args[0])))));

        operations.Add("ceil", (new[] {
            LOC.Get("PARAM_CellOp_Ceil_Hint_1")}, 
            LOC.Get("PARAM_CellOp_Ceil_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Ceiling(v))));

        operations.Add("floor", (new[] { 
            LOC.Get("PARAM_CellOp_Floor_Hint_1")}, 
            LOC.Get("PARAM_CellOp_Floor_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Floor(v))));

        operations.Add("prepend", (new[] { 
            LOC.Get("PARAM_CellOp_Prepend_Hint_1")}, 
            LOC.Get("PARAM_CellOp_Prepend_TT"),
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => $"{args[0]}{v}"))
        );
    }
}
