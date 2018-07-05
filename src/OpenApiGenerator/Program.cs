using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using System.Net.Http;

namespace OpenApiGenerator
{
    class Program
    {
        static string _yamlOutputFile = "output/swagger.yaml";
        static string _jsonOutputFile = "output/swagger.json";
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader sr = File.OpenText(@"spec\swagger.yaml"))
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

                AddComponentSchemas();
                AddPaths();

                // output a json file
                using (StreamReader sr = File.OpenText(@"output\swagger.yaml"))
                {
                    var httpClient = new HttpClient
                    {
                        BaseAddress = new Uri("https://raw.githubusercontent.com/OAI/OpenAPI-Specification/")
                    };

                    var stream = httpClient.GetStreamAsync("master/examples/v3.0/petstore.yaml").GetAwaiter().GetResult();

                    var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);

                    using (TextWriter writer = File.CreateText(_jsonOutputFile))
                    {
                        openApiDocument.SerializeAsV3(new OpenApiJsonWriter(writer));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        static void AddComponentSchemas()
        {
            var yamlSchemaFiles = Directory.GetFiles(@"spec\components\schemas\", "*.yaml", SearchOption.AllDirectories);

            using (TextWriter writer = File.AppendText(_yamlOutputFile))
            {
                writer.WriteLine("components:");
                writer.WriteLine("\tschemas:");
            }

            foreach (var file in yamlSchemaFiles)
            {
                var fileInfo = new FileInfo(file);

                using (StreamReader sr = new StreamReader(file))
                {
                    using (TextWriter writer = File.AppendText(_yamlOutputFile))
                    {
                        writer.WriteLine($"\t\t{fileInfo.Name.Substring(0, fileInfo.Name.IndexOf("."))}:");

                        var tabs = "\t\t\t";
                        var s = "";

                        while ((s = sr.ReadLine()) != null)
                        {
                            writer.WriteLine($"{tabs}{s}");
                        }
                    }
                }
            }
        }

        static void AddPaths()
        {
            var yamlPathFiles = Directory.GetFiles(@"spec\paths\", "*.yaml", SearchOption.AllDirectories);

            using (TextWriter writer = File.AppendText(_yamlOutputFile))
            {
                writer.WriteLine("paths:");
            }

            foreach (var file in yamlPathFiles)
            {
                var fileInfo = new FileInfo(file);

                using (StreamReader sr = new StreamReader(file))
                {
                    using (TextWriter writer = File.AppendText(_yamlOutputFile))
                    {
                        var path = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(".")).Replace("@", "/");
                        writer.WriteLine($"\t'/{path}':");

                        var tabs = "\t\t";
                        var s = "";

                        while ((s = sr.ReadLine()) != null)
                        {
                            writer.WriteLine($"{tabs}{s}");
                        }
                    }
                }
            }
        }
    }
}