$fileNames = Get-ChildItem -Path $scriptPath

foreach ($file in $fileNames)
{
    if ($file.Name.EndsWith("vert") -Or $file.Name.EndsWith("frag") -Or $file.Name.EndsWith("comp"))
    {
        Write-Host "Compiling $file"
        glslangvalidator -V $file -o $file".spv"
    }
}

# The wireframe view's SimpleFlver variants: the same vertex stage, SimpleFlver.geom, and the fragment stage with WIREFRAME
foreach ($set in @("SimpleFlver", "SimpleFlver_selected", "SimpleFlver_focus"))
{
    Write-Host "Compiling $set _wire"
    glslangvalidator -V SimpleFlver.vert -o $set"_wire.vert.spv"
    glslangvalidator -V SimpleFlver.geom -o $set"_wire.geom.spv"
    glslangvalidator -V -DWIREFRAME $set".frag" -o $set"_wire.frag.spv"
}

..\..\..\Veldrid.SPIRV.VariantCompiler\bin\Release\net6.0-windows\Veldrid.SPIRV.VariantCompiler.exe --search-path .\FlverShader --output-path .\FlverShader --set .\FlverShader\FlverShader.json