# Project
Smithbox is a all purpose editor for FromSoftware games. 
It provides a database-like model for editing game data, and 3D editors for maps and models.

# Limitation
Do not delete files or folders. Instead move them to the .archive folder.
Do not create or force-push git commits.

# Game Data
The game data locations are found in the game_data.txt file.
For ER, use the VFS functionality from Smithbox to access the files.

# Tools
Create projects in the Smithbox/Claude/Tools folder for temporary tools, rather than using the scratchpad.
Use C# when writing temporary tools, except when intructed to use other languages.
Do not use Python.

# Plans
Create planning and analysis markdown (.MD) files in the Smithbox/Claude/Documentation folder.

# Verification
Do not try to verify changes by launching the program. Pass to the user and tell them what to check for. If additional information is needed during this step, include temporary changes to print additional diagnostics to a log that the user can pass back.

# Process
If a decision is needed, ask one decision at a time, with 2 to 4 concrete options, one marked recommended, a short rationale each. Use the structured-choice tool if one is available; otherwise ask in one short message.
