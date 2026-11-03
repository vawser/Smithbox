# Project
Smithbox is a all purpose editor for FromSoftware games. 
It provides a database-like model for editing game data, and 3D editors for maps and models.

# Limitation
Do not delete files or folders. Instead move them to the .archive folder.
Do not create or force-push git commits.

# Game Data
These are the folder locations for game data for each of the game supported by Smithbox:

AC6: "F:\SteamLibrary\steamapps\common\ARMORED CORE VI FIRES OF RUBICON\Game"
DS2: "F:\SteamLibrary\steamapps\common\Dark Souls II\Game"
DS2S: "F:\SteamLibrary\steamapps\common\Dark Souls II Scholar of the First Sin\Game"
DS3: "F:\SteamLibrary\steamapps\common\DARK SOULS III\Game"
DS1: "F:\SteamLibrary\steamapps\common\Dark Souls Prepare to Die Edition\DATA"
DS1R: "F:\SteamLibrary\steamapps\common\DARK SOULS REMASTERED"
ER: "F:\SteamLibrary\steamapps\common\ELDEN RING\Game"
NR: "F:\SteamLibrary\steamapps\common\ELDEN RING NIGHTREIGN\Game"
SDT: "F:\SteamLibrary\steamapps\common\Sekiro"
BB: "G:\Game Data Repository\Bloodborne"
DES: "G:\Game Data Repository\Demon's Souls\PS3_GAME\USRDIR"
DB_NT: "G:\Game Data Repository\Duskbloods\0400d1d02399a000-BebTjl\base\romfs"

For ER, use the VFS functionality from Smithbox to access the files.

# Tools
Do not try to use Python.
Use C# when writing temporary tools.
Remember to escape characters when using regex with bash or in the shell.

# Verification
Do not try to verify changes by launching the program. Pass to the user and tell them what to check for. If additional information is needed during this step, include tempoary changes to print additional diagnosics to a log that the user can pass back.

# Process
If a decision is needed, ask one decision at a time, with 2 to 4 concrete options, one marked recommended, a short rationale each. Use the structured-choice tool if one is available; otherwise ask in one short message.