using System.Collections.Generic;
using System.Linq;
using LuaDecompilerCore.IR;

namespace LuaDecompilerCore.Passes;

/// <summary>
/// Try to replace string or number env/act IDs with constant defines
/// </summary>
/*public class AnnotateEnvActFunctionsPass : IPass
{
    public void RunOnFunction(DecompilationContext context, Function f)
    {
        var envJapaneseMap = new Dictionary<string, string>();
        var envIdMap = new Dictionary<int, string>();
        var nameIdentifierMap = new Dictionary<string, Identifier>();
        foreach (var env in Annotations.ESDFunctions.ESDEnvs)
        {
            envJapaneseMap.Add(env.JapaneseName, env.EnglishEnum);
            envIdMap.Add(env.ID, env.EnglishEnum);
            var id = new Identifier
            {
                Name = env.EnglishEnum,
                Type = Identifier.IdentifierType.Global
            };
            nameIdentifierMap.Add(env.EnglishEnum, id);
        }

        foreach (var b in f.BlockList)
        {
            foreach (var i in b.Instructions)
            {
                foreach (var e in i.GetExpressions())
                {
                    if (e is FunctionCall { Function: IdentifierReference ir } call && ir.Identifier.Name == "env")
                    {
                        if (call.Args.Count > 0)
                        {
                            switch (call.Args[0])
                            {
                                case Constant { ConstType: Constant.ConstantType.ConstString } c1:
                                {
                                    if (envJapaneseMap.TryGetValue(c1.String, out var value))
                                    {
                                        call.Args[0] = new IdentifierReference(nameIdentifierMap[value]);
                                    }

                                    break;
                                }
                                case Constant { ConstType: Constant.ConstantType.ConstNumber } c2:
                                {
                                    if (envIdMap.TryGetValue((int)c2.Number, out var value))
                                    {
                                        call.Args[0] = new IdentifierReference(nameIdentifierMap[value]);
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}*/