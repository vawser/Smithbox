#------------------------------
# Adding a New Language
#------------------------------
To support a new language in Smithbox, add a new entry to the Languages.json

UICultures: https://www.venea.net/web/locale

#------------------------------
# Adding Localization Entries
#------------------------------
Create a folder for your new language. You can then create any number of json files, following this structure:

{
    "Entries": [ 
        { "Key": "EXAMPLE_KEY", "Text": "Key" },
    ]
}

#------------------------------
# Naming
#------------------------------
When naming the class, the standard pattern is:

1. an all-CAPS term
(i.e. PROJECT for Project code, MAP for Map Editor code, etc)

2. a CamelCase term
(i.e. ConfigureMenu for Project Configure Menu)

3. a short descriptive term for the text, in CamelCase, with underlines for spaces
(i.e. Create_Mod_Profile

For tooltips, add _TT to the end of the key 
(i.e. BUTTON_TEXT is used for the button, therefore BUTTON_TEXT_TT should be used for the tooltip)

#------------------------------
# Parameterized Text
#------------------------------
If the text has parameters that are passed in via C#, use the following notation:

{1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}

The number is which parameter is used for said slot.

#------------------------------
# Notes
#------------------------------
Duplicate keys in entries will be ignored. Therefore only the first instance of a key will be used.