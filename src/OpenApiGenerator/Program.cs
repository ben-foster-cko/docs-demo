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
        static string _yamlOutputFile = "../../output/swagger.yaml";
        static string _jsonOutputFile = "../../output/swagger.json";
        static string _specDir = "../../spec";
        static void Main(string[] args)
        {
            try
            {
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

                AddPaths();
                AddComponentSchemas();

                // output a json file
                var str = File.ReadAllText(_yamlOutputFile);

                var openApiDocument = new OpenApiStringReader().Read(str, out var diagnostic);

                foreach (var error in diagnostic.Errors)
                {
                    Console.WriteLine(error.Message);
                    Console.WriteLine(error.Pointer);
                }

                using (TextWriter writer = File.CreateText(_jsonOutputFile))
                {
                    openApiDocument.SerializeAsV3(new OpenApiJsonWriter(writer));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void AddComponentSchemas()
        {
            var yamlSchemaFiles = Directory.GetFiles($"{_specDir}/components/schemas/", "*.yaml", SearchOption.AllDirectories);

            var text = "components:\n  schemas:\n";

            foreach (var file in yamlSchemaFiles)
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
            File.AppendAllText(_yamlOutputFile, text, Encoding.UTF8);
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