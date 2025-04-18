using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace Veldrid.SPIRV
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CommandLineApplication.Execute<Program>(args);
        }

        [Option("--search-path", "The set of directories to search for shader source files.", CommandOptionType.MultipleValue)]
        public string[] SearchPaths { get; }

        [Option("--output-path", "The directory where compiled files are placed.", CommandOptionType.SingleValue)]
        public string OutputPath { get; }

        [Option("--set", "The path to the JSON file containing shader variant definitions to compile.", CommandOptionType.SingleValue)]
        public string SetDefinitionPath { get; }

        public void OnExecute()
        {
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }

            ShaderVariantDescription[] descs;
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            StringEnumConverter enumConverter = new StringEnumConverter();
            serializer.Converters.Add(enumConverter);
            using (StreamReader sr = File.OpenText(SetDefinitionPath))
            using (JsonTextReader jtr = new JsonTextReader(sr))
            {
                descs = serializer.Deserialize<ShaderVariantDescription[]>(jtr);
            }

            HashSet<string> generatedPaths = new HashSet<string>();

            VariantCompiler compiler = new VariantCompiler(new List<string>(SearchPaths), OutputPath);
            foreach (ShaderVariantDescription desc in descs)
            {
                string[] newPaths = compiler.Compile(desc);
                foreach (string s in newPaths)
                {
                    generatedPaths.Add(s);
                }
            }

            string generatedFilesListText = string.Join(Environment.NewLine, generatedPaths);
            string generatedFilesListPath = Path.Combine(OutputPath, "vspv_generated_files.txt");
            File.WriteAllText(generatedFilesListPath, generatedFilesListText);
        }
    }
}
