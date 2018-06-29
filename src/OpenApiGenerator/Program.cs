using System;
using System.IO;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections.Generic;
using SharpYaml.Serialization;
using System.Linq;

namespace OpenApiGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                OpenApiDocument document;
                using (StreamReader sr = new StreamReader(@"..\..\spec\swaggerTest.yaml"))
                {
                    document = new OpenApiStringReader().Read(sr.ReadToEnd(), out var diag);
                }

                if (document != null)
                {
                    AddComponentSchemas(document);

                    using (TextWriter writer = File.CreateText("spec.txt"))
                    {
                        document.SerializeAsV3(new OpenApiYamlWriter(writer));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void AddComponentSchemas(OpenApiDocument document)
        {
            var yamlSchemaFiles = Directory.GetFiles(@"..\..\spec\components\schemas\", "*.yaml*", SearchOption.AllDirectories);

            var schemas = new Dictionary<string, OpenApiSchema>();
            foreach (var file in yamlSchemaFiles)
            {
                var fileInfo = new FileInfo(file);
                using (StreamReader sr = new StreamReader(file))
                {
                    var yaml = new YamlStream();
                    yaml.Load(sr);

                    var rootNode = (YamlMappingNode)yaml.Documents[0].RootNode;
                    var schema = new OpenApiSchema();

                    SetPropertyFieldValues(rootNode.Children, schema);

                    schemas.Add(fileInfo.Name.Substring(0, fileInfo.Name.IndexOf(".")), schema);
                }

                document.Components = new OpenApiComponents { Schemas = schemas };
            }
        }

        static void SetProperties(IOrderedDictionary<YamlNode, YamlNode> properties, OpenApiSchema parentOpenApiSchema)
        {
            foreach (var property in properties)
            {
                var propertyName = ((YamlScalarNode)property.Key).Value;
                var propertyFields = ((YamlMappingNode)property.Value).Children;

                var openApiSchema = new OpenApiSchema();
                SetPropertyFieldValues(propertyFields, openApiSchema);
                parentOpenApiSchema.Properties.Add(propertyName, openApiSchema);
            }
        }

        static void SetPropertyFieldValues(IOrderedDictionary<YamlNode, YamlNode> children, OpenApiSchema propertySchema)
        {
            var typeNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "type");
            var descriptionNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "description");
            var exampleNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "example");
            var patternNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "pattern");
            var requiredNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "required");
            var allOfNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "allOf");
            var itemsNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "items");
            var propertiesNode = children.FirstOrDefault(x => ((YamlScalarNode)x.Key).Value == "properties");

            if (typeNode.Value != null)
                propertySchema.Type = ((YamlScalarNode)typeNode.Value).Value;

            if (descriptionNode.Value != null)
                propertySchema.Description = ((YamlScalarNode)descriptionNode.Value).Value;

            if (exampleNode.Value != null)
            {
                if (exampleNode.Value as YamlMappingNode != null && ((YamlMappingNode)exampleNode.Value).Children != null)
                {
                    var openApiObject = new OpenApiObject();
                    foreach (var exampleField in ((YamlMappingNode)exampleNode.Value).Children)
                    {
                        var key = ((YamlScalarNode)exampleField.Key).Value;
                        var openApiElement = new OpenApiString(((YamlScalarNode)exampleField.Value).Value);
                        openApiObject.Add(key, openApiElement);
                    }
                    propertySchema.Example = openApiObject;
                }
                else
                {
                    propertySchema.Example = new OpenApiString(((YamlScalarNode)exampleNode.Value).Value);
                }
            }

            if (itemsNode.Value != null)
            {
                var itemsSchema = new OpenApiSchema();
                SetPropertyFieldValues(((YamlMappingNode)itemsNode.Value).Children, itemsSchema);
                propertySchema.Items = itemsSchema;
            }

            if (allOfNode.Value != null)
            {
                var reference = ((YamlSequenceNode)allOfNode.Value).Children.FirstOrDefault();
                var externalResource = ((YamlScalarNode)((YamlMappingNode)reference).FirstOrDefault().Value).Value;
                var allOfSchema = new OpenApiSchema { Reference = new OpenApiReference { ExternalResource = externalResource } };
                propertySchema.AllOf = new List<OpenApiSchema> { allOfSchema };
            }

            if (patternNode.Value != null)
                propertySchema.Pattern = ((YamlScalarNode)patternNode.Value).Value;

            if (requiredNode.Value != null)
            {
                propertySchema.Required = new HashSet<string>();
                var fieldNodes = ((YamlMappingNode)propertiesNode.Value).Children;
                foreach (var fieldNode in fieldNodes)
                {
                    var requiredValue = ((YamlScalarNode)fieldNode.Key).Value;
                    propertySchema.Required.Add(requiredValue);
                }
            }

            if (propertiesNode.Value != null)
            {
                var properties = ((YamlMappingNode)propertiesNode.Value);
                var propchildren = properties.Children;
                SetProperties(propchildren, propertySchema);
            }
        }
    }
}
