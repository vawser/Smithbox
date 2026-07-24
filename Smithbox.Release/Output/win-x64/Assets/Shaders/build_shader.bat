@echo off
for %%F in (%*) do (
    echo Processing: %%F
    glslang.exe --target-env vulkan1.3 "%%F" -o temp.spv
    
    REM Rename the output file based on the input file's extension
    if "%%~xF"==".frag" ren "temp.spv" "%%~nF.frag.spv"
    if "%%~xF"==".vert" ren "temp.spv" "%%~nF.vert.spv"
)
pause