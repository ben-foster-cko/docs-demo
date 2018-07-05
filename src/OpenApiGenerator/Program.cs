using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace OpenApiGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                using (StreamReader sr = File.OpenText(@"..\..\spec\swagger.yaml"))
                {
                    using (TextWriter writer = File.CreateText("spec.txt"))
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
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void AddComponentSchemas()
        {
            var yamlSchemaFiles = Directory.GetFiles(@"..\..\spec\components\schemas\", "*.yaml", SearchOption.AllDirectories);

            using (TextWriter writer = File.AppendText("spec.txt"))
            {
                writer.WriteLine("components:");
                writer.WriteLine("\tschemas:");
            }

            foreach (var file in yamlSchemaFiles)
            {
                var fileInfo = new FileInfo(file);

                using (StreamReader sr = new StreamReader(file))
                {
                    using (TextWriter writer = File.AppendText("spec.txt"))
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
            var yamlPathFiles = Directory.GetFiles(@"..\..\spec\paths\", "*.yaml", SearchOption.AllDirectories);

            using (TextWriter writer = File.AppendText("spec.txt"))
            {
                writer.WriteLine("paths:");
            }

            foreach (var file in yamlPathFiles)
            {
                var fileInfo = new FileInfo(file);

                using (StreamReader sr = new StreamReader(file))
                {
                    using (TextWriter writer = File.AppendText("spec.txt"))
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