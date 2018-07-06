using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using System.Net.Http;
using System.Text;

namespace OpenApiGenerator
{
    class Program
    {
        static string _outputDir = "output";
        static string _yamlOutputFile = "output/swagger.yaml";
        static string _jsonOutputFile = "output/swagger.json";
        static string _specDir = "spec";

        static void Main(string[] args)
        {
            try
            {
                RefreshOutputDirectory();

                // start building up the yaml file
                using (StreamReader sr = File.OpenText($"{_specDir}/swagger.yaml"))
                {
                    using (TextWriter writer = File.CreateText(_yamlOutputFile))
                    {
                        var s = "";
                        while ((s = sr.ReadLine()) != null)
                        {
                            writer.WriteLine(s);
                        }
                    }
                }

                // append paths and components to yaml file
                AddPaths();
                AddAllComponents();

                // use openapi.net to read yaml file
                var str = File.ReadAllText(_yamlOutputFile);
                var openApiDocument = new OpenApiStringReader().Read(str, out var diagnostic);

                // log any errors
                foreach (var error in diagnostic.Errors)
                {
                    Console.WriteLine(error.Message);
                    Console.WriteLine(error.Pointer);
                }

                // convert yaml file to json
                using (TextWriter writer = File.CreateText(_jsonOutputFile))
                {
                    openApiDocument.SerializeAsV3(new OpenApiJsonWriter(writer));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        static void RefreshOutputDirectory()
        {
            ClearOutputDirectory();
            Directory.CreateDirectory(_outputDir);
        }

        static void ClearOutputDirectory()
        {
            if (Directory.Exists(_outputDir))
            {
                foreach (var file in Directory.GetFiles(_outputDir))
                {
                    File.Delete(file);
                }

                Directory.Delete(_outputDir);
            }
        }

        static void AddAllComponents()
        {
            File.AppendAllText(_yamlOutputFile, "components:\n", Encoding.UTF8);

            foreach (var component in new List<string> { "schemas", "headers", "parameters", "responses", "securitySchemes" })
            {
                AddComponent(component);
            }
        }

        static void AddComponent(string component)
        {
            var yamlSchemaFiles = Directory.GetFiles($"{_specDir}/components/{component}/", "*.yaml", SearchOption.AllDirectories);
            var text = $"  {component}:\n";
            text += GetComponentsText(yamlSchemaFiles);
            File.AppendAllText(_yamlOutputFile, text, Encoding.UTF8);
        }

        static string GetComponentsText(string[] yamlFiles)
        {
            var text = "";

            foreach (var file in yamlFiles)
            {
                var fileInfo = new FileInfo(file);

                using (StreamReader sr = new StreamReader(file))
                {
                    var path = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf("."));
                    text += ($"    {path}:\n");

                    var tabs = "      ";
                    var s = "";

                    while ((s = sr.ReadLine()) != null)
                    {
                        text += $"{tabs}{s}\n";
                    }
                }
            }

            return text;
        }

        static void AddPaths()
        {
            var yamlPathFiles = Directory.GetFiles($"{_specDir}/paths/", "*.yaml", SearchOption.AllDirectories);

            var text = "paths:\n";

            foreach (var file in yamlPathFiles)
            {
                var fileInfo = new FileInfo(file);

                using (StreamReader sr = new StreamReader(file))
                {
                    var path = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(".")).Replace("@", "/");
                    text += ($"  /{path}:\n");

                    var tabs = "    ";
                    var s = "";

                    while ((s = sr.ReadLine()) != null)
                    {
                        text += $"{tabs}{s}\n";
                    }
                }
            }

            File.AppendAllText(_yamlOutputFile, text, Encoding.UTF8);
        }
    }
}