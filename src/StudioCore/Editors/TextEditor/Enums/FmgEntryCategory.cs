using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Enums;

/// <summary>
/// Text categories used for grouping multiple FMGs or broad identification
/// </summary>
public enum FmgEntryCategory
{
    None = -1,
    Goods,
    Weapons,
    Armor,
    Rings,
    Spells,
    Characters,
    Locations,
    Gem,
    Message,
    SwordArts,
    Effect,
    ActionButtonText,
    Tutorial,
    LoadingScreen,
    Generator,
    Booster,
    FCS,
    Mission,
    Archive,

    ItemFmgDummy = 200 // Anything with this will be sorted into the item section of the editor.
}
