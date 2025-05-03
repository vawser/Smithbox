using Hexa.NET.ImGui;
using StudioCore.Editors.ParamEditor;
using StudioCore.Interface;
using System.Numerics;

namespace StudioCore.Editor;

public class UIHints
{
    public static string MassEditHint =
        @"For help with regex or examples, consult the main help menu.
Mass Edit Commands exist to make large batch-edits according to a simple scheme.
Specific help with individual components of massedit can be found in the autofill menu by 
clicking the ?

The autofill is a valuable tool to help understand massedits. Feel free to experiment, 
you can ctrl-z a massedit.

A mass edit command is formed of selectors and an operation.
There are multiple stages of selection when making a param massedit, going from params, 
to rows, to cells (fields).

Finally, an operation is applied to everything selected.

A selector has a name, such as the row selector 'prop', and can have arguments,
separated by spaces.

In this case, prop expects the property name, and a value that the row should have to be selected.
All rows which match this are kept, while everything else is discarded.
This process repeats for each condition, and across each stage of selection.
There exists a special case of selector, which combines both param and row selectors into one key word.
Most commonly this is the 'selection' selector, which selects currently selected rows in the currently selected param.
Unlike the search bar selections, selections in massedit apply stricter criteria, needing to match exactly.

An operation also has a name, though often this is a symbol, such as '+', and has arguments.
An operation's arguments begin after the first space, but are actually separated with another : when there are multiple.
This is because the argument may also have arguments itself, separated with spaces.
For example, the argument 'field weight'. Instead of a fixed number, for each param, row and cell evaluated, a value to use is determined.
In this case, the number to add depends on the row we're currently in, and retrieves a different weight value each row.

The following is for advanced topics, and requires you to enable advanced options in the settings.

Massedit can do more than modify final values of the currently loaded params.
Afer selecting rows, it is possible to perform a row operation instead of selecting cells/fields.
These operations act on rows instead, including copying them into clipboard, or pasting them into the current params.
There exists also global operations, which require no selectors beforehand, such as clear to clear the clipboard.
The clipboard only supports 1 param at a time, and can be selected from in the same manner as current UI selection.

When an additional set of params (parambank) is loaded, aux selectors and arguments become available.
With these, one can make a selection of not the currently loaded params, but from an auxilliary set.
While it is not ideal to modify these with cell operations, they are useful for row operations, including passing rows between parambanks.

Massedit can also define variables for use, mostly in scripts. A variable's type is dictated when defined in a global operation.
It can be an integer, floating point number, or a string.
A variable's current value can be accessed using $name, and is usable in selector args, op args, and even the args of those op args.
Sets of vars can be selected in the same manner as params, and can be modified with the same operations available to edit param fields.";

    public static string SearchBarHint =
        @"For help with regex or examples, consult the main help menu.
This searchbar utilise Regex, and words surrounded by ! in commands indicate that a Regex expression may be used instead of plain text.
All other words in capitals are parameters for the given command.
Regex searches are case-insensitive and the searched term may appear anywhere in the target rows. To specify an exact match, surround the text with ^ and $ (eg. ^10$) or use a range variant.
Multiple selectors can be given by separating them with &&.

Row selection is done through any of the following:
    !VALUE!: to select rows with a matching ID or a matching name
    modified: to select rows changed from vanilla
    !modified: to select unmodified rows
    id !VALUE!: to select rows with a matching ID
    idrange MIN MAX: to select rows with an ID within the given bounds
    name !NAME!: to select rows with a matching name
    prop FIELD !VALUE!: to select rows who have a matching value for the given field
    proprange FIELD MIN MAX: to select rows who have a value for the given field within the given bounds
    propref FIELD !NAME!: to select rows that have a reference to another row with a matching name.

A complete search may look like the following examples:
id 10000 (This searches for all rows with an id containing 10000. This includes 10000, 1000010, 210000)
name Dagger (This searches for all rows with a name containing Dagger. This includes Blood Dagger, Sharp daggers and daggerfall)
propref originEquipWep0 Dagger (This searches for all rows whose field originEquipWep0 refers to a row with a name containing Dagger, following the same rules above.
name Dagger && idrange 10000 Infinity (This searches for all rows with a name containing Dagger and that have an id greater than 9999)";

    public static string MassEditExamples = @"A complete MassEdit command may look like the following examples:
selection: throwAtkRate: = 30; (This selects from the current selection, the field throwAtkRate and makes its value 30)
param EquipParamWeapon: name Dagger.*: throwAtkRate: * 2; (This selects from EquipParamWeapon all rows beginning with Dagger, and multiplies the values in throwAtkRate by 2)
param EquipParamWeapon: prop weaponCategory 0: correctAgility: + field correctStrength; (This selects from EquipParamWeapon all rows whose weaponCategory is 0, and adds the row's correctStrength to its correctAgility)
param EquipParamWeapon: id .*: name: replace Dark:Holy; (This selects from EquipParamWeapon ALL rows, and replaces all cases of Dark in their name with Holy
param EquipParamWeapon: name Dagger.* && idrange 10000 Infinity: throwAtkRate: * 2; (This selects from EquipParamWeapon all rows beginning with Dagger and with an id higher than 9999, and multiplies the values in throwAtkRate by 2)";

    public static string SearchExamples = @"A complete search may look like the following examples:
id 10000 (This searches for all rows with an id containing 10000. This includes 10000, 1000010, 210000)
name Dagger (This searches for all rows with a name containing Dagger. This includes Blood Dagger, Sharp daggers and daggerfall)
propref originEquipWep0 Dagger (This searches for all rows whose field originEquipWep0 refers to a row with a name containing Dagger, following the same rules above.
name Dagger && idrange 10000 Infinity (This searches for all rows with a name containing Dagger and that have an id greater than 9999)";

    public static string RegexCheatSheet =
        @"Regex is a common way to write an expression that 'matches' strings or words, finding occurances within a passage of text.

For letters and numbers, regex matches only those explicit characters and nothing else. This means searching for dog, you will only match when 'dog' is found.
Regex also has many meta-characters, and most symbols have some meaning. For example, * means 'any number of the previous character'.
In this case, Do*g matches Dg, Dog, Doog, Dooog... etc.

Regex can also provide options with the | symbol (OR symbol). Dog|Frog matches Dog or Frog.
This isn't always useful as maybe only part of your expression is optional. You can use brackets to seperate that part.
In this manner, (D|Fr)og matches Dog or Frog. You can even use * on a bracketed expression.

Sometimes it doesn't make sense to write (0|1|2|3|4|5|6|7|8|9), so regex also has something called ranges.
This is written as [abc123*+] and matches any single character inside. Many metacharacters also don't function inside, and merely match themselves literally.
They can also include expressions like [1-9] and [a-zA-Z] which include the middle numbers and letters.

As if that wasn't enough, for common ones of these there are more shorthands called character classes.
\d matches [0-9], \w matches any letter, number, or _, \s matches any whitespace, and . matches any character at all - which makes .* a handy 'match any amount of anything'.
Character classes can also begin with ^ to mean any character NOT given. So [^0] is any nonzero character.

Finally, regex is often used to find occurances within text, not to match the entire text. The character ^ is used to match the start of the text, and $ the end.
A statement like ^[^-] will only match text that begins with something other than - for example.

Regex has a broad syntax and application, but with this quick guide and an online cheatsheet, many interesting combinations are possible.
While regex is aimed at text and is not perfect for numbers, it can still be useful.
Some common tools for mapstudio include:
.* (match anything)
^10$ (match only when the entire input is 10)
[^0] (match anything that isn't just 0)
^[^-] (match anything that doesn't begin with -)
^2\d\d$ (match any number from 200 to 299)";

    public static bool AddImGuiHintButton(string id, ref string hint, bool canEdit = false, bool isRowHint = false)
    {
        var scale = DPI.GetUIScale();
        var ret = false;
        /*
        ImGui.TextColored(new Vector4(0.6f, 0.6f, 1.0f, 1.0f), "Help");
        if (ImGui.BeginPopupContextItem(id))
        {
            if (ParamEditor.ParamEditorScreen.EditorMode && canEdit) //remove this, editor mode should be called earlier
            {
                ImGui.InputTextMultiline("", ref hint, 8196, new Vector2(720, 480));
                if (ImGui.IsItemDeactivatedAfterEdit())
                    ret = true;
            }
            else
                ImGui.Text(hint);
            ImGui.EndPopup();
        }
        */

        // Even worse of a hack than it was before. eat my shorts (all of this should be redone)
        if (isRowHint)
        {
            ImGui.TextColored(new Vector4(0.6f, 0.6f, 1.0f, 1f), "?");
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                ImGui.OpenPopup(id);
            }

            if (ImGui.BeginPopupContextItem(id))
            {
                ImGui.Text(hint);

                ImGui.EndPopup();
            }

            ImGui.SameLine();
        }
        else
        {
            ImGui.SameLine(0, 20f);
            if (ImGui.Button("Help"))
            {
                ImGui.OpenPopup("##ParamHelp");
            }

            if (ImGui.BeginPopup("##ParamHelp"))
            {
                ImGui.Text(hint);

                ImGui.EndPopup();
            }
        }

        return ret;
    }
}
