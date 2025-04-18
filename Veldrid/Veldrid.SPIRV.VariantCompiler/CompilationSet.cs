using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vortice.Vulkan;

namespace Veldrid.SPIRV
{
    public class VariantStageDescription
    {
        public VkShaderStageFlags Stage { get; }
        public string FileName { get; }

        public VariantStageDescription(VkShaderStageFlags stage, string fileName)
        {
            Stage = stage;
            FileName = fileName;
        }
    }

    public class ShaderVariantDescription
    {
        public string Name { get; }
        public VariantStageDescription[] Shaders { get; }
        public MacroDefinition[] Macros { get; }
        public CrossCompileOptions CrossCompileOptions { get; }
        public CrossCompileTarget[] Targets { get; }

        public ShaderVariantDescription(
            string name,
            VariantStageDescription[] shaders,
            MacroDefinition[] macros,
            CrossCompileOptions crossCompileOptions,
            CrossCompileTarget[] targets)
        {
            Name = name;
            Shaders = shaders;
            Macros = macros;
            CrossCompileOptions = crossCompileOptions;
            Targets = targets;
        }
    }

    public class VariantCompiler
    {
        private readonly List<string> _shaderSearchPaths = new List<string>();
        private readonly string _outputPath;

        public VariantCompiler(List<string> shaderSearchPaths, string outputPath)
        {
            _shaderSearchPaths = shaderSearchPaths;
            _outputPath = outputPath;
        }

        public string[] Compile(ShaderVariantDescription variant)
        {
            if (variant.Shaders.Length == 1)
            {
                if (variant.Shaders[0].Stage == VkShaderStageFlags.Vertex) { return CompileVertexFragment(variant); }
                if (variant.Shaders[0].Stage == VkShaderStageFlags.Compute) { return CompileCompute(variant); }
            }
            if (variant.Shaders.Length == 2)
            {
                bool hasVertex = false;
                bool hasFragment = false;
                foreach (var shader in variant.Shaders)
                {
                    hasVertex |= shader.Stage == VkShaderStageFlags.Vertex;
                    hasFragment |= shader.Stage == VkShaderStageFlags.Fragment;
                }

                if (!hasVertex)
                {
                    throw new SpirvCompilationException($"Variant \"{variant.Name}\" is missing a vertex shader.");
                }
                if (!hasFragment)
                {
                    throw new SpirvCompilationException($"Variant \"{variant.Name}\" is missing a fragment shader.");
                }

                return CompileVertexFragment(variant);
            }
            else
            {
                throw new SpirvCompilationException(
                    $"Variant \"{variant.Name}\" has an unsupported combination of shader stages.");
            }
        }

        private string[] CompileVertexFragment(ShaderVariantDescription variant)
        {
            List<string> generatedFiles = new List<string>();
            List<Exception> compilationExceptions = new List<Exception>();
            byte[] vsBytes = null;
            byte[] fsBytes = null;

            string vertexFileName = variant.Shaders.FirstOrDefault(vsd => vsd.Stage == VkShaderStageFlags.Vertex)?.FileName;
            if (vertexFileName != null)
            {
                try
                {
                    vsBytes = CompileToSpirv(variant, vertexFileName, VkShaderStageFlags.Vertex);
                    //string spvPath = Path.Combine(_outputPath, $"{variant.Name}_{ShaderStages.Vertex.ToString()}.spv");
                    string spvPath = Path.Combine(_outputPath, $"{variant.Name}.vert.spv");
                    File.WriteAllBytes(spvPath, vsBytes);
                    generatedFiles.Add(spvPath);
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            string fragmentFileName = variant.Shaders.FirstOrDefault(vsd => vsd.Stage == VkShaderStageFlags.Fragment)?.FileName;
            if (fragmentFileName != null)
            {
                try
                {
                    fsBytes = CompileToSpirv(variant, fragmentFileName, VkShaderStageFlags.Fragment);
                    //string spvPath = Path.Combine(_outputPath, $"{variant.Name}_{ShaderStages.Fragment.ToString()}.spv");
                    string spvPath = Path.Combine(_outputPath, $"{variant.Name}.frag.spv");
                    File.WriteAllBytes(spvPath, fsBytes);
                    generatedFiles.Add(spvPath);
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            if (compilationExceptions.Count > 0)
            {
                throw new AggregateException(
                    $"Errors were encountered when compiling from GLSL to SPIR-V.",
                    compilationExceptions);
            }

            foreach (CrossCompileTarget target in variant.Targets)
            {
                try
                {
                    bool writeReflectionFile = true;
                    VertexFragmentCompilationResult result = SpirvCompilation.CompileVertexFragment(
                        vsBytes,
                        fsBytes,
                        target,
                        variant.CrossCompileOptions);
                    if (result.VertexShader != null)
                    {
                        string vsPath = Path.Combine(_outputPath, $"{variant.Name}_Vertex.{GetExtension(target)}");
                        File.WriteAllText(vsPath, result.VertexShader);
                        generatedFiles.Add(vsPath);
                    }
                    if (result.FragmentShader != null)
                    {
                        string fsPath = Path.Combine(_outputPath, $"{variant.Name}_Fragment.{GetExtension(target)}");
                        File.WriteAllText(fsPath, result.FragmentShader);
                        generatedFiles.Add(fsPath);
                    }

                    if (writeReflectionFile)
                    {
                        writeReflectionFile = false;
                        string reflectionPath = Path.Combine(_outputPath, $"{variant.Name}_ReflectionInfo.json");

                        JsonSerializer serializer = new JsonSerializer();
                        serializer.Formatting = Formatting.Indented;
                        StringEnumConverter enumConverter = new StringEnumConverter();
                        serializer.Converters.Add(enumConverter);
                        using (StreamWriter sw = File.CreateText(reflectionPath))
                        using (JsonTextWriter jtw = new JsonTextWriter(sw))
                        {
                            serializer.Serialize(jtw, result.Reflection);
                        }
                        generatedFiles.Add(reflectionPath);
                    }
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            if (compilationExceptions.Count > 0)
            {
                throw new AggregateException($"Errors were encountered when compiling shader variant(s).", compilationExceptions);
            }

            return generatedFiles.ToArray();
        }

        private string GetExtension(CrossCompileTarget target)
        {
            switch (target)
            {
                case CrossCompileTarget.HLSL:
                    return "hlsl";
                case CrossCompileTarget.GLSL:
                    return "glsl";
                case CrossCompileTarget.ESSL:
                    return "essl";
                case CrossCompileTarget.MSL:
                    return "metal";
                default:
                    throw new SpirvCompilationException($"Invalid CrossCompileTarget: {target}");
            }
        }

        private byte[] CompileToSpirv(
            ShaderVariantDescription variant,
            string fileName,
            VkShaderStageFlags stage)
        {
            GlslCompileOptions glslOptions = GetOptions(variant);
            string glsl = LoadGlsl(fileName);
            SpirvCompilationResult result = SpirvCompilation.CompileGlslToSpirv(
                glsl,
                fileName,
                stage,
                glslOptions);
            return result.SpirvBytes;
        }

        private GlslCompileOptions GetOptions(ShaderVariantDescription variant)
        {
            return new GlslCompileOptions(true, variant.Macros);
        }

        private string LoadGlsl(string fileName)
        {
            if (fileName == null) { return null; }

            foreach (string searchPath in _shaderSearchPaths)
            {
                string fullPath = Path.Combine(searchPath, fileName);
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
            }

            throw new FileNotFoundException($"Unable to find shader file \"{fileName}\".");
        }

        private string[] CompileCompute(ShaderVariantDescription variant)
        {
            List<string> generatedFiles = new List<string>();
            byte[] csBytes = CompileToSpirv(variant, variant.Shaders[0].FileName, VkShaderStageFlags.Compute);
            string spvPath = Path.Combine(_outputPath, $"{variant.Name}_{VkShaderStageFlags.Compute.ToString()}.spv");
            File.WriteAllBytes(spvPath, csBytes);
            generatedFiles.Add(spvPath);

            List<Exception> compilationExceptions = new List<Exception>();
            foreach (CrossCompileTarget target in variant.Targets)
            {
                try
                {
                    ComputeCompilationResult result = SpirvCompilation.CompileCompute(csBytes, target, variant.CrossCompileOptions);
                    string csPath = Path.Combine(_outputPath, $"{variant.Name}_Compute.{GetExtension(target)}");
                    File.WriteAllText(csPath, result.ComputeShader);
                    generatedFiles.Add(csPath);

                    string reflectionPath = Path.Combine(_outputPath, $"{variant.Name}_ReflectionInfo.json");

                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Formatting = Formatting.Indented;
                    StringEnumConverter enumConverter = new StringEnumConverter();
                    serializer.Converters.Add(enumConverter);
                    using (StreamWriter sw = File.CreateText(reflectionPath))
                    using (JsonTextWriter jtw = new JsonTextWriter(sw))
                    {
                        serializer.Serialize(jtw, result.Reflection);
                    }
                    generatedFiles.Add(reflectionPath);
                }
                catch (Exception e)
                {
                    compilationExceptions.Add(e);
                }
            }

            if (compilationExceptions.Count > 0)
            {
                throw new AggregateException($"Errors were encountered when compiling shader variant(s).", compilationExceptions);
            }

            return generatedFiles.ToArray();
        }
    }
}
