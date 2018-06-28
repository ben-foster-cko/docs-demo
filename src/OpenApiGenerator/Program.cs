using System;
using System.IO;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;

namespace OpenApiGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader(@"..\..\spec\swagger.1.yaml"))
                {
                    var document = new OpenApiStringReader().Read(sr.ReadToEnd(), out var diag);

                    using (TextWriter writer = File.CreateText("spec.txt"))
                    {
                        document.SerializeAsV3(new OpenApiYamlWriter(writer));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }
    }
}
