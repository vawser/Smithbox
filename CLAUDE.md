# Project
Smithbox is a all purpose editor for FromSoftware games. 
It provides a database-like model for editing game data, and 3D editors for maps and models.

# Limitation
Do not delete files or folders. Instead move them to the .archive folder.
Do not create or force-push git commits.

# UI
Do not use ImGuiInputTextFlags.EnterReturnsTrue: it confuses users, since an edit is lost when they click away instead of pressing Enter.
Apply a text field when it is deactivated instead: call GUI.InputCommitted() right after the InputText (true when the field is left after an edit, by Enter, Tab or a click, but not by Escape).
Beside a Cancel button, check ImGui.IsItemActive() right after drawing Cancel and skip the commit if true (the click that left the field was on Cancel).
In a dialog with several inputs, don't apply the dialog on a field's deactivation (moving to the next field would apply it): use the Apply button only.

# Game Data
The game data locations are found in the game_data.txt file.
For ER, use the VFS functionality from Smithbox to access the files.

# Tools
Use C# when writing temporary tools, except when intructed to use other languages.
Do not use Python.
If you need to use Oodle, the DLL file can be found in Smithbox/Claude
Create projects in the Smithbox/Claude/Tools folder for temporary tools.
Place temporary files in the Smithbox/Claude/Tools folder

# Plans
Create planning and analysis markdown (.MD) files in the Smithbox/Claude/Documentation folder.

# Logs
When creating logs for diagnostics or other reasons, place them in the Smithbox/Claude/Logs folder.

# Verification
Do not try to verify changes by launching the program. Pass to the user and tell them what to check for. If additional information is needed during this step, include temporary changes to print additional diagnostics to a log that the user can pass back.

# Process
If a decision is needed, ask one decision at a time, with 2 to 4 concrete options, one marked recommended, a short rationale each. Use the structured-choice tool if one is available; otherwise ask in one short message.
