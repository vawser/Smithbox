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
        operations.Add("=",
            (new[] { "number or text" },
                "Assigns the given value to the selected values. Will attempt conversion to the value's data type",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => args[0])));

        operations.Add("+", (new[] { "number or text" },
            "Adds the number to the selected values, or appends text if that is the data type of the values",
            (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v =>
            {
                double val;
                if (double.TryParse(args[0], out val))
                {
                    return v + val;
                }

                return v + args[0];
            })));

        operations.Add("-",
            (new[] { "number" }, "Subtracts the number from the selected values",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v - double.Parse(args[0]))));

        operations.Add("*",
            (new[] { "number" }, "Multiplies selected values by the number",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v * double.Parse(args[0]))));

        operations.Add("/",
            (new[] { "number" }, "Divides the selected values by the number",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v / double.Parse(args[0]))));

        operations.Add("%",
            (new[] { "number" }, "Gives the remainder when the selected values are divided by the number",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v % double.Parse(args[0]))));

        operations.Add("scale", (new[] { "factor number", "center number" },
            "Multiplies the difference between the selected values and the center number by the factor number",
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

        operations.Add("replace",
            (new[] { "text to replace", "new text" },
                "Interprets the selected values as text and replaces all occurances of the text to replace with the new text",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => v.Replace(args[0], args[1]))));

        operations.Add("replacex", (new[] { "text to replace (regex)", "new text (w/ groups)" },
            "Interprets the selected values as text and replaces all occurances of the given regex with the replacement, supporting regex groups",
            (ctx, args) =>
            {
                Regex rx = new(args[0]);
                return MassParamEdit.WithDynamicOf(ctx, v => rx.Replace(v, args[1]));
            }
        ));

        operations.Add("max",
            (new[] { "number" }, "Returns the larger of the current value and number",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Max(v, double.Parse(args[0])))));

        operations.Add("min",
            (new[] { "number" }, "Returns the smaller of the current value and number",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Min(v, double.Parse(args[0])))));

        operations.Add("round",
            (new[] { "number" }, "Rounds the current value to the specified number of decimals",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Round(v, int.Parse(args[0])))));

        operations.Add("ceil",
            (new[] { "number" }, "Rounds the current value up to the closest integer",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Ceiling(v))));

        operations.Add("floor",
            (new[] { "number" }, "Rounds the current value down to the closest integer",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => Math.Floor(v))));

        operations.Add("prepend",
            (new[] { "text to prepend" }, "Prepends the text to the current text value",
                (ctx, args) => MassParamEdit.WithDynamicOf(ctx, v => $"{args[0]}{v}"))
        );
    }
}
